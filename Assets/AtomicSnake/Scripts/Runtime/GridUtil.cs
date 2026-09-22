using UnityEngine;

namespace AtomicSnake.Runtime
{
    public static class GridUtil
    {
        public static Vector2 RandomPointInField(float margin = 0.6f)
        {
            float halfWidth = GridConfig.Columns * GridConfig.CellSize / 2f;
            float halfHeight = GridConfig.Rows * GridConfig.CellSize / 2f;
            return new Vector2(
                Random.Range(-halfWidth + margin, halfWidth - margin),
                Random.Range(-halfHeight + margin, halfHeight - margin));
        }

        public static bool IsOutOfBounds(Vector2 position)
        {
            float halfWidth = GridConfig.Columns * GridConfig.CellSize / 2f;
            float halfHeight = GridConfig.Rows * GridConfig.CellSize / 2f;
            return Mathf.Abs(position.x) > halfWidth || Mathf.Abs(position.y) > halfHeight;
        }
    }
}
