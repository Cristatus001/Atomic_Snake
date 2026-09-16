using System;
using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Snake
{
    // Continuous movement: arrow keys/WASD pick a cardinal target heading and
    // the head steers smoothly toward it (Mathf.MoveTowardsAngle) instead of
    // snapping between grid cells. The body is drawn as one smooth curve
    // (LineRenderer) tracing the head's recent path, so turns look like a
    // real snake instead of a blocky zig-zag.
    public class SnakeController : MonoBehaviour
    {
        public event Action<Vector2> OnHeadMoved;
        public event Action OnSelfCollision;
        public event Action OnHitWall;

        const float Speed = 3.4f;
        const float TurnRateDegreesPerSec = 240f;
        const float GrowthPerPickup = 0.5f;      // world units of body length added per correct pickup
        const float TrailSampleSpacing = 0.03f;
        const float BodyThickness = 0.5f;
        const float SelfCollisionDistance = 0.28f;
        const float SelfCollisionSkipDistance = 0.5f; // ignore the neck right behind the head

        static readonly Color HeadColor = new Color(0.16f, 0.38f, 0.86f);
        static readonly Color BodyColor = new Color(0.30f, 0.68f, 0.36f);

        Transform _container;
        SpriteRenderer _headVisual;
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

            EnsureVisuals();
            RefreshVisuals();
            _active = true;
        }

        void EnsureVisuals()
        {
            if (_headVisual == null)
            {
                var go = new GameObject("Head");
                go.transform.SetParent(_container, false);
                _headVisual = go.AddComponent<SpriteRenderer>();
                _headVisual.sortingOrder = 12;
                _headVisual.sprite = SpriteFactory.GetCircle(HeadColor);
            }
            SpriteFactory.SetWorldSize(_headVisual, BodyThickness * 1.15f);

            if (_bodyLine == null)
            {
                var go = new GameObject("BodyLine");
                go.transform.SetParent(_container, false);
                _bodyLine = go.AddComponent<LineRenderer>();
                _bodyLine.useWorldSpace = false;
                _bodyLine.numCapVertices = 8;
                _bodyLine.numCornerVertices = 8;
                _bodyLine.material = new Material(Shader.Find("Sprites/Default"));
                _bodyLine.sortingOrder = 10;
            }
            _bodyLine.startColor = BodyColor;
            _bodyLine.endColor = BodyColor;
            _bodyLine.widthMultiplier = BodyThickness;
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
            _headVisual.transform.localPosition = _headPosition;

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
