using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Field
{
    // A simple placeholder garden-plot ground plane with raised hedge-like
    // border walls, sized to the continuous play field.
    public static class FieldBackground
    {
        static readonly Color Soil = new Color(0.75f, 0.58f, 0.36f);
        static readonly Color Hedge = new Color(0.24f, 0.47f, 0.27f);

        const float WallHeight = 0.4f;
        const float WallThickness = 0.3f;

        public static void Build(Transform parent)
        {
            ShapeFactory.CreateGroundQuad(parent, Soil, FieldConfig.Width, FieldConfig.Depth, "Ground");

            float halfW = FieldConfig.Width / 2f;
            float halfD = FieldConfig.Depth / 2f;
            float wallY = WallHeight / 2f;

            CreateWall(parent, new Vector3(0f, wallY, halfD + WallThickness / 2f), new Vector3(FieldConfig.Width + WallThickness * 2f, WallHeight, WallThickness));
            CreateWall(parent, new Vector3(0f, wallY, -halfD - WallThickness / 2f), new Vector3(FieldConfig.Width + WallThickness * 2f, WallHeight, WallThickness));
            CreateWall(parent, new Vector3(halfW + WallThickness / 2f, wallY, 0f), new Vector3(WallThickness, WallHeight, FieldConfig.Depth));
            CreateWall(parent, new Vector3(-halfW - WallThickness / 2f, wallY, 0f), new Vector3(WallThickness, WallHeight, FieldConfig.Depth));
        }

        static void CreateWall(Transform parent, Vector3 localPosition, Vector3 size)
        {
            var wall = ShapeFactory.CreateBox(parent, Hedge, size, "HedgeWall");
            wall.transform.localPosition = localPosition;
        }
    }
}
