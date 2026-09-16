using UnityEngine;

namespace AtomicSnake.Runtime
{
    // The field is a continuous 3D ground plane (X = width, Z = depth, Y = up),
    // not a discrete grid — the snake moves and turns smoothly across it.
    public static class FieldConfig
    {
        public const float Width = 12f;
        public const float Depth = 7.5f;

        public const float SnakeSpeed = 3.4f;
        public const float TurnRateDegreesPerSec = 240f;

        public const float SegmentSpacing = 0.32f;
        public const float SegmentRadius = 0.26f;
        public const float HeadRadius = 0.32f;

        public const float SelfCollisionDistance = 0.30f;
        public const float PickupDistance = 0.46f;

        public const float TileRadius = 0.30f;
        public const float TileMinSeparation = 0.9f;

        public static Vector3 RandomPointInField(float margin = 0.6f)
        {
            float x = Random.Range(-Width / 2f + margin, Width / 2f - margin);
            float z = Random.Range(-Depth / 2f + margin, Depth / 2f - margin);
            return new Vector3(x, 0f, z);
        }

        public static bool IsOutOfBounds(Vector3 position)
        {
            return Mathf.Abs(position.x) > Width / 2f || Mathf.Abs(position.z) > Depth / 2f;
        }
    }
}
