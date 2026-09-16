using System;
using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Snake
{
    // Classic grid-stepping snake: moves one cell per tick, wraps at the
    // field edges, and reports self-collision so the caller can react.
    public class SnakeController : MonoBehaviour
    {
        public event Action<Vector2Int> OnHeadMoved;
        public event Action OnSelfCollision;

        [SerializeField] float moveInterval = 0.18f;

        static readonly Color HeadColor = new Color(0.16f, 0.38f, 0.86f);
        static readonly Color BodyColor = new Color(0.30f, 0.68f, 0.36f);

        readonly List<Vector2Int> _cells = new();
        readonly List<SpriteRenderer> _segments = new();

        Vector2Int _direction;
        Vector2Int _pendingDirection;
        int _pendingGrowth;
        float _tickTimer;
        Transform _container;
        bool _active;

        public IReadOnlyList<Vector2Int> Cells => _cells;
        public Vector2Int HeadCell => _cells[0];
        public int Length => _cells.Count;

        public void Initialize(Transform container)
        {
            _container = container;
        }

        public void ResetSnake(Vector2Int startCell, int startLength, Vector2Int startDirection)
        {
            foreach (var segment in _segments)
                Destroy(segment.gameObject);
            _segments.Clear();
            _cells.Clear();

            _direction = startDirection;
            _pendingDirection = startDirection;
            _pendingGrowth = 0;
            _tickTimer = 0f;

            for (int i = 0; i < startLength; i++)
                _cells.Add(GridUtil.Wrap(startCell - startDirection * i));

            for (int i = 0; i < startLength; i++)
                _segments.Add(CreateSegmentVisual());

            RefreshVisuals();
            _active = true;
        }

        public void SetActive(bool value) => _active = value;

        public void QueueGrowth(int amount = 1) => _pendingGrowth += amount;

        public void TrySetDirection(Vector2Int dir)
        {
            if (dir == Vector2Int.zero || dir == -_direction) return;
            _pendingDirection = dir;
        }

        public bool OccupiesCell(Vector2Int cell) => _cells.Contains(cell);

        SpriteRenderer CreateSegmentVisual()
        {
            var go = new GameObject("SnakeSegment");
            go.transform.SetParent(_container, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.GetCircle(BodyColor);
            sr.sortingOrder = 10;
            return sr;
        }

        void Update()
        {
            if (!_active) return;

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= moveInterval)
            {
                _tickTimer -= moveInterval;
                Step();
            }
        }

        void Step()
        {
            _direction = _pendingDirection;
            var newHead = GridUtil.Wrap(_cells[0] + _direction);
            bool willGrow = _pendingGrowth > 0;

            bool collided = false;
            int bodyLimit = willGrow ? _cells.Count : _cells.Count - 1;
            for (int i = 0; i < bodyLimit; i++)
            {
                if (_cells[i] != newHead) continue;
                collided = true;
                break;
            }

            _cells.Insert(0, newHead);
            if (willGrow)
            {
                _pendingGrowth--;
                _segments.Insert(0, CreateSegmentVisual());
            }
            else
            {
                _cells.RemoveAt(_cells.Count - 1);
                var tailSegment = _segments[^1];
                _segments.RemoveAt(_segments.Count - 1);
                _segments.Insert(0, tailSegment);
            }

            RefreshVisuals();
            OnHeadMoved?.Invoke(newHead);
            if (collided)
                OnSelfCollision?.Invoke();
        }

        void RefreshVisuals()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                var sr = _segments[i];
                sr.transform.localPosition = GridUtil.CellToWorld(_cells[i]);
                bool isHead = i == 0;
                sr.sprite = SpriteFactory.GetCircle(isHead ? HeadColor : BodyColor);
                sr.sortingOrder = isHead ? 12 : 10;
                SpriteFactory.SetWorldSize(sr, GridConfig.CellSize * (isHead ? 0.92f : 0.8f));
            }
        }
    }
}
