using System;
using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Snake
{
    // Continuous, slither-style movement: the head steers smoothly toward a
    // cardinal target heading (set by arrow-key/WASD input) and every body
    // segment follows the head's recently-traveled path, so turns curve
    // naturally instead of snapping between grid cells.
    public class SnakeController : MonoBehaviour
    {
        public event Action<Vector3> OnHeadMoved;
        public event Action OnSelfCollision;
        public event Action OnHitWall;

        const float TrailSampleSpacing = 0.03f;
        const int SkipNearestSegmentsForCollision = 4;

        static readonly Color HeadColor = new Color(0.16f, 0.38f, 0.86f);
        static readonly Color BodyColor = new Color(0.30f, 0.68f, 0.36f);

        Transform _container;
        Transform _headVisual;
        readonly List<Transform> _segmentVisuals = new();
        readonly List<Vector3> _segmentPositions = new();
        readonly List<Vector3> _trail = new();

        Vector3 _headPosition;
        float _heading;
        float _targetHeading;
        int _segmentCount;
        int _pendingGrowth;
        bool _active;

        public Vector3 HeadPosition => _headPosition;
        public IReadOnlyList<Vector3> SegmentPositions => _segmentPositions;

        public void Initialize(Transform container)
        {
            _container = container;
        }

        public void ResetSnake(Vector3 startPosition, int startLength, float startHeadingDegrees)
        {
            if (_headVisual != null) Destroy(_headVisual.gameObject);
            foreach (var seg in _segmentVisuals)
                if (seg != null) Destroy(seg.gameObject);
            _segmentVisuals.Clear();
            _segmentPositions.Clear();
            _trail.Clear();

            _headPosition = startPosition;
            _heading = startHeadingDegrees;
            _targetHeading = startHeadingDegrees;
            _segmentCount = startLength;
            _pendingGrowth = 0;

            var backward = HeadingToDirection(startHeadingDegrees + 180f);
            float totalBack = (startLength + 2) * FieldConfig.SegmentSpacing;
            for (float d = totalBack; d >= 0f; d -= TrailSampleSpacing)
                _trail.Add(startPosition + backward * d);

            _headVisual = ShapeFactory.CreateSphere(_container, HeadColor, FieldConfig.HeadRadius * 2f, "Head").transform;
            for (int i = 0; i < startLength; i++)
                _segmentVisuals.Add(ShapeFactory.CreateSphere(_container, BodyColor, FieldConfig.SegmentRadius * 2f).transform);

            RefreshVisuals();
            _active = true;
        }

        public void SetActive(bool value) => _active = value;
        public void QueueGrowth(int amount = 1) => _pendingGrowth += amount;
        public void SetTargetHeading(float degrees) => _targetHeading = degrees;

        void Update()
        {
            if (!_active) return;

            _heading = Mathf.MoveTowardsAngle(_heading, _targetHeading, FieldConfig.TurnRateDegreesPerSec * Time.deltaTime);
            var direction = HeadingToDirection(_heading);
            _headPosition += direction * FieldConfig.SnakeSpeed * Time.deltaTime;

            AppendTrail(_headPosition);

            if (_pendingGrowth > 0)
            {
                _pendingGrowth--;
                _segmentCount++;
                _segmentVisuals.Add(ShapeFactory.CreateSphere(_container, BodyColor, FieldConfig.SegmentRadius * 2f).transform);
            }

            RefreshVisuals();
            _headVisual.rotation = Quaternion.LookRotation(direction, Vector3.up);

            OnHeadMoved?.Invoke(_headPosition);

            if (FieldConfig.IsOutOfBounds(_headPosition))
                OnHitWall?.Invoke();
            else if (CheckSelfCollision())
                OnSelfCollision?.Invoke();
        }

        void AppendTrail(Vector3 newPos)
        {
            if (_trail.Count == 0 || Vector3.Distance(_trail[^1], newPos) >= TrailSampleSpacing)
                _trail.Add(newPos);

            float maxNeeded = (_segmentCount + 2) * FieldConfig.SegmentSpacing + 0.5f;
            float total = 0f;
            int cut = 0;
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                total += Vector3.Distance(_trail[i], _trail[i - 1]);
                if (total > maxNeeded)
                {
                    cut = i - 1;
                    break;
                }
            }
            if (cut > 0) _trail.RemoveRange(0, cut);
        }

        Vector3 PositionAtDistanceBehindHead(float distance)
        {
            if (_trail.Count == 0) return _headPosition;

            float remaining = distance;
            Vector3 prevPoint = _headPosition;
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                Vector3 point = _trail[i];
                float segLen = Vector3.Distance(prevPoint, point);
                if (segLen >= remaining)
                {
                    float t = segLen > 0f ? remaining / segLen : 0f;
                    return Vector3.Lerp(prevPoint, point, t);
                }
                remaining -= segLen;
                prevPoint = point;
            }
            return _trail[0];
        }

        void RefreshVisuals()
        {
            _headVisual.localPosition = _headPosition + Vector3.up * FieldConfig.HeadRadius;

            _segmentPositions.Clear();
            for (int i = 0; i < _segmentVisuals.Count; i++)
            {
                float distance = (i + 1) * FieldConfig.SegmentSpacing;
                var pos = PositionAtDistanceBehindHead(distance);
                _segmentPositions.Add(pos);
                _segmentVisuals[i].localPosition = pos + Vector3.up * FieldConfig.SegmentRadius;
            }
        }

        bool CheckSelfCollision()
        {
            for (int i = SkipNearestSegmentsForCollision; i < _segmentPositions.Count; i++)
            {
                if (Vector3.Distance(_headPosition, _segmentPositions[i]) < FieldConfig.SelfCollisionDistance)
                    return true;
            }
            return false;
        }

        static Vector3 HeadingToDirection(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
        }
    }
}
