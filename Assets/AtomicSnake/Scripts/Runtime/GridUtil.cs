using UnityEngine;

namespace AtomicSnake.Runtime
{
    public static class GridUtil
    {
        // Used only for the background dressing (soil rows); the snake and
        // tiles now live in continuous space, not on discrete cells.
        public static Vector3 CellToWorld(Vector2Int cell)
        {
            float originX = -(GridConfig.Columns - 1) * GridConfig.CellSize * 0.5f;
            float originY = -(GridConfig.Rows - 1) * GridConfig.CellSize * 0.5f;
            return new Vector3(originX + cell.x * GridConfig.CellSize, originY + cell.y * GridConfig.CellSize, 0f);
        }

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
