using System;
using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Snake
{
    // Continuous movement: arrow keys/WASD pick a cardinal target heading and
    // the head steers smoothly toward it (Mathf.MoveTowardsAngle) instead of
    // snapping between grid cells. The body is a single LineRenderer tracing
    // the head's recent path. All sprite/material/color presentation lives
    // on the SnakeHead and SnakeBody prefabs (assigned below), so an artist
    // can restyle either without touching this script.
    public class SnakeController : MonoBehaviour
    {
        public event Action<Vector2> OnHeadMoved;
        public event Action OnSelfCollision;
        public event Action OnHitWall;

        [SerializeField] Transform headPrefab;
        [SerializeField] LineRenderer bodyPrefab;

        const float Speed = 3.4f;
        const float TurnRateDegreesPerSec = 240f;
        const float GrowthPerPickup = 0.5f;      // world units of body length added per correct pickup
        const float TrailSampleSpacing = 0.03f;
        const float SelfCollisionDistance = 0.28f;
        const float SelfCollisionSkipDistance = 0.5f; // ignore the neck right behind the head

        Transform _container;
        Transform _head;
        LineRenderer _bodyLine;

        Vector2 _headPosition;
        float _heading;
        float _targetHeading;
        float _bodyLength;
        bool _active;

        readonly List<Vector2> _trail = new();
        readonly List<Vector2> _bodySamples = new();

        public Vector2 HeadPosition => _headPosition;
        public IReadOnlyList<Vector2> BodySamples => _bodySamples;

        public void Initialize(Transform container)
        {
            _container = container;
            _head = Instantiate(headPrefab, _container);
            _bodyLine = Instantiate(bodyPrefab, _container);
        }

        public void ResetSnake(Vector2 startPosition, int startLength, float startHeadingDegrees)
        {
            _trail.Clear();
            _bodySamples.Clear();

            _headPosition = startPosition;
            _heading = startHeadingDegrees;
            _targetHeading = startHeadingDegrees;
            _bodyLength = startLength * GrowthPerPickup;

            var backward = HeadingToDirection(startHeadingDegrees + 180f);
            float totalBack = _bodyLength + 1f;
            for (float d = totalBack; d >= 0f; d -= TrailSampleSpacing)
                _trail.Add(startPosition + backward * d);

            RefreshVisuals();
            _active = true;
        }

        public void SetActive(bool value) => _active = value;
        public void QueueGrowth(int amount = 1) => _bodyLength += amount * GrowthPerPickup;
        public void SetTargetHeading(float degrees) => _targetHeading = degrees;

        void Update()
        {
            if (!_active) return;

            _heading = Mathf.MoveTowardsAngle(_heading, _targetHeading, TurnRateDegreesPerSec * Time.deltaTime);
            var direction = HeadingToDirection(_heading);
            _headPosition += direction * Speed * Time.deltaTime;

            AppendTrail(_headPosition);
            RefreshVisuals();

            OnHeadMoved?.Invoke(_headPosition);

            if (GridUtil.IsOutOfBounds(_headPosition))
                OnHitWall?.Invoke();
            else if (CheckSelfCollision())
                OnSelfCollision?.Invoke();
        }

        void AppendTrail(Vector2 newPos)
        {
            if (_trail.Count == 0 || Vector2.Distance(_trail[^1], newPos) >= TrailSampleSpacing)
                _trail.Add(newPos);

            float maxNeeded = _bodyLength + 0.5f;
            float total = 0f;
            int cut = 0;
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                total += Vector2.Distance(_trail[i], _trail[i - 1]);
                if (total > maxNeeded)
                {
                    cut = i - 1;
                    break;
                }
            }
            if (cut > 0) _trail.RemoveRange(0, cut);
        }

        void RefreshVisuals()
        {
            _head.localPosition = _headPosition;

            _bodySamples.Clear();
            _bodySamples.Add(_headPosition);

            float remaining = _bodyLength;
            Vector2 prevPoint = _headPosition;
            for (int i = _trail.Count - 1; i >= 0 && remaining > 0f; i--)
            {
                Vector2 point = _trail[i];
                float segLen = Vector2.Distance(prevPoint, point);
                if (segLen >= remaining)
                {
                    float t = segLen > 0f ? remaining / segLen : 0f;
                    _bodySamples.Add(Vector2.Lerp(prevPoint, point, t));
                    remaining = 0f;
                    break;
                }
                _bodySamples.Add(point);
                remaining -= segLen;
                prevPoint = point;
            }

            _bodyLine.positionCount = _bodySamples.Count;
            for (int i = 0; i < _bodySamples.Count; i++)
                _bodyLine.SetPosition(i, _bodySamples[i]);
        }

        bool CheckSelfCollision()
        {
            float traveled = 0f;
            for (int i = 1; i < _bodySamples.Count; i++)
            {
                traveled += Vector2.Distance(_bodySamples[i - 1], _bodySamples[i]);
                if (traveled < SelfCollisionSkipDistance) continue;

                if (Vector2.Distance(_headPosition, _bodySamples[i]) < SelfCollisionDistance)
                    return true;
            }
            return false;
        }

        static Vector2 HeadingToDirection(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
