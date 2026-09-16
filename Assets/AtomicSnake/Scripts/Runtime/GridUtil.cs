using UnityEngine;

namespace AtomicSnake.Runtime
{
    public static class GridUtil
    {
        public static Vector2Int Wrap(Vector2Int cell)
        {
            int x = ((cell.x % GridConfig.Columns) + GridConfig.Columns) % GridConfig.Columns;
            int y = ((cell.y % GridConfig.Rows) + GridConfig.Rows) % GridConfig.Rows;
            return new Vector2Int(x, y);
        }

        public static Vector3 CellToWorld(Vector2Int cell)
        {
            float originX = -(GridConfig.Columns - 1) * GridConfig.CellSize * 0.5f;
            float originY = -(GridConfig.Rows - 1) * GridConfig.CellSize * 0.5f;
            return new Vector3(originX + cell.x * GridConfig.CellSize, originY + cell.y * GridConfig.CellSize, 0f);
        }

        public static Vector2Int RandomCell()
        {
            return new Vector2Int(Random.Range(0, GridConfig.Columns), Random.Range(0, GridConfig.Rows));
        }
    }
}
