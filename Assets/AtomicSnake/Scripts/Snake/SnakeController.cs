using System;
using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Snake
{
    // Continuous movement: arrow keys/WASD pick a cardinal target heading and
    // the head steers smoothly toward it (Mathf.MoveTowardsAngle) instead of
    // snapping between grid cells. The field wraps at the edges like classic
    // Snake, so the body is drawn as one or more LineRenderer "runs" — a new
    // run starts wherever the trail crosses a wrap seam, so the body never
    // draws a line straight across the field when it re-enters the opposite side.
    // All sprite/material/color presentation lives on the SnakeHead and
    // SnakeBody prefabs (assigned below), so an artist can restyle either
    // without touching this script.
    public class SnakeController : MonoBehaviour
    {
        public event Action<Vector2> OnHeadMoved;
        public event Action OnSelfCollision;

        [SerializeField] Transform headPrefab;
        [SerializeField] LineRenderer bodyPrefab;

        const float DefaultSpeed = 3.4f;
        const float TurnRateDegreesPerSec = 240f;
        const float GrowthPerPickup = 0.5f;      // world units of body length added per correct pickup
        const float TrailSampleSpacing = 0.03f;
        const float SelfCollisionDistance = 0.28f;
        const float SelfCollisionSkipDistance = 0.5f; // ignore the neck right behind the head

        Transform _container;
        Transform _head;
        readonly List<LineRenderer> _bodyLinePool = new();

        Vector2 _headPosition;
        float _heading;
        float _targetHeading;
        float _bodyLength;
        float _speed = DefaultSpeed;
        bool _active;

        // Parallel to _trail: _trailSeam[i] true means the segment between
        // _trail[i-1] and _trail[i] is a wrap seam (a teleport, not real
        // travel), so it must not be drawn or counted as body-length distance.
        readonly List<Vector2> _trail = new();
        readonly List<bool> _trailSeam = new();

        readonly List<List<Vector2>> _bodyRuns = new();
        readonly List<Vector2> _bodySamples = new(); // flat view of all runs, for collision/spawn-avoidance queries

        public Vector2 HeadPosition => _headPosition;
        public IReadOnlyList<Vector2> BodySamples => _bodySamples;

        public void Initialize(Transform container)
        {
            _container = container;
            _head = Instantiate(headPrefab, _container);
        }

        public void ResetSnake(Vector2 startPosition, int startLength, float startHeadingDegrees)
        {
            _trail.Clear();
            _trailSeam.Clear();

            _headPosition = startPosition;
            _heading = startHeadingDegrees;
            _targetHeading = startHeadingDegrees;
            _bodyLength = startLength * GrowthPerPickup;
            _speed = DefaultSpeed;

            var backward = HeadingToDirection(startHeadingDegrees + 180f);
            float totalBack = _bodyLength + 1f;
            for (float d = totalBack; d >= 0f; d -= TrailSampleSpacing)
            {
                _trail.Add(startPosition + backward * d);
                _trailSeam.Add(false);
            }

            RefreshVisuals();
            _active = true;
        }

        public void SetActive(bool value) => _active = value;
        public void QueueGrowth(int amount = 1) => _bodyLength += amount * GrowthPerPickup;
        public void SetTargetHeading(float degrees) => _targetHeading = degrees;
        public void SetSpeed(float unitsPerSecond) => _speed = unitsPerSecond > 0f ? unitsPerSecond : DefaultSpeed;

        void Update()
        {
            if (!_active) return;

            _heading = Mathf.MoveTowardsAngle(_heading, _targetHeading, TurnRateDegreesPerSec * Time.deltaTime);
            var direction = HeadingToDirection(_heading);
            var rawPosition = _headPosition + direction * _speed * Time.deltaTime;
            bool wrapped = GridUtil.IsOutOfBounds(rawPosition);
            _headPosition = wrapped ? GridUtil.Wrap(rawPosition) : rawPosition;

            AppendTrail(_headPosition, wrapped);
            RefreshVisuals();

            OnHeadMoved?.Invoke(_headPosition);

            if (CheckSelfCollision())
                OnSelfCollision?.Invoke();
        }

        void AppendTrail(Vector2 newPos, bool wrapped)
        {
            if (_trail.Count == 0 || wrapped || Vector2.Distance(_trail[^1], newPos) >= TrailSampleSpacing)
            {
                _trail.Add(newPos);
                _trailSeam.Add(wrapped);
            }

            float maxNeeded = _bodyLength + 0.5f;
            float total = 0f;
            int cut = 0;
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                float segLen = _trailSeam[i] ? 0f : Vector2.Distance(_trail[i], _trail[i - 1]);
                total += segLen;
                if (total > maxNeeded)
                {
                    cut = i - 1;
                    break;
                }
            }
            if (cut > 0)
            {
                _trail.RemoveRange(0, cut);
                _trailSeam.RemoveRange(0, cut);
            }
        }

        void RefreshVisuals()
        {
            _head.localPosition = _headPosition;

            _bodyRuns.Clear();
            var currentRun = new List<Vector2> { _headPosition };
            _bodyRuns.Add(currentRun);

            float remaining = _bodyLength;
            Vector2 prevPoint = _headPosition;

            for (int i = _trail.Count - 1; i >= 0 && remaining > 0f; i--)
            {
                Vector2 point = _trail[i];
                bool seam = i + 1 < _trailSeam.Count && _trailSeam[i + 1];

                if (seam)
                {
                    // The segment we'd otherwise draw (prevPoint -> point) is a
                    // wrap teleport: end the current run and start a new one,
                    // without consuming any of the remaining body length.
                    currentRun = new List<Vector2> { point };
                    _bodyRuns.Add(currentRun);
                    prevPoint = point;
                    continue;
                }

                float segLen = Vector2.Distance(prevPoint, point);
                if (segLen >= remaining)
                {
                    float t = segLen > 0f ? remaining / segLen : 0f;
                    currentRun.Add(Vector2.Lerp(prevPoint, point, t));
                    remaining = 0f;
                    break;
                }

                currentRun.Add(point);
                remaining -= segLen;
                prevPoint = point;
            }

            _bodySamples.Clear();
            for (int r = 0; r < _bodyRuns.Count; r++)
            {
                EnsureBodyLine(r);
                var run = _bodyRuns[r];
                _bodySamples.AddRange(run);

                var line = _bodyLinePool[r];
                line.gameObject.SetActive(true);
                line.positionCount = run.Count;
                for (int i = 0; i < run.Count; i++)
                    line.SetPosition(i, run[i]);
            }
            for (int r = _bodyRuns.Count; r < _bodyLinePool.Count; r++)
                _bodyLinePool[r].gameObject.SetActive(false);
        }

        void EnsureBodyLine(int index)
        {
            while (_bodyLinePool.Count <= index)
                _bodyLinePool.Add(Instantiate(bodyPrefab, _container));
        }

        bool CheckSelfCollision()
        {
            for (int r = 0; r < _bodyRuns.Count; r++)
            {
                var run = _bodyRuns[r];
                float traveled = 0f;
                for (int i = 1; i < run.Count; i++)
                {
                    traveled += Vector2.Distance(run[i - 1], run[i]);
                    if (r == 0 && traveled < SelfCollisionSkipDistance) continue;

                    if (Vector2.Distance(_headPosition, run[i]) < SelfCollisionDistance)
                        return true;
                }
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
