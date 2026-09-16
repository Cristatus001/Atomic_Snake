using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Field
{
    // Builds a simple placeholder garden-plot look: alternating soil-row
    // strips inside a green hedge border, sized to the play grid.
    public static class FieldBackground
    {
        static readonly Color SoilLight = new Color(0.78f, 0.62f, 0.40f);
        static readonly Color SoilDark = new Color(0.71f, 0.55f, 0.35f);
        static readonly Color Hedge = new Color(0.24f, 0.47f, 0.27f);

        public static void Build(Transform parent)
        {
            float width = GridConfig.Columns * GridConfig.CellSize;
            float height = GridConfig.Rows * GridConfig.CellSize;
            float border = GridConfig.CellSize * 0.5f;

            CreateQuad(parent, "Hedge", Hedge, Vector3.zero, width + border * 2f, height + border * 2f, -0.02f, 1);

            for (int row = 0; row < GridConfig.Rows; row++)
            {
                var color = row % 2 == 0 ? SoilLight : SoilDark;
                var rowCenter = GridUtil.CellToWorld(new Vector2Int(0, row));
                var pos = new Vector3(0f, rowCenter.y, -0.01f);
                CreateQuad(parent, $"Row_{row}", color, pos, width, GridConfig.CellSize, -0.01f, 0);
            }
        }

        static GameObject CreateQuad(Transform parent, string name, Color color, Vector3 localPosition, float width, float height, float z, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPosition.x, localPosition.y, z);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.GetSolid(color);
            sr.sortingOrder = sortingOrder;
            SpriteFactory.SetWorldSize(sr, width, height);
            return go;
        }
    }
}
