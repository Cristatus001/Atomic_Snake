using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Field
{
    // A single collectible word-part tile (e.g. "un", "nil", "ium") floating
    // above one point in the continuous field.
    public class WordTileView : MonoBehaviour
    {
        static readonly Color TileColor = new Color(0.86f, 0.55f, 0.70f);

        public string Text { get; private set; }
        public Vector3 Position { get; private set; }

        public static WordTileView Create(Transform parent, string text, Vector3 position)
        {
            var go = ShapeFactory.CreateSphere(parent, TileColor, FieldConfig.TileRadius * 2f, $"Tile_{text}");
            go.transform.localPosition = position + Vector3.up * FieldConfig.TileRadius;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, FieldConfig.TileRadius + 0.22f, 0f);
            var mesh = labelGo.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.12f;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;

            var billboard = labelGo.AddComponent<Billboard>();
            billboard.Target = Camera.main;

            var tile = go.AddComponent<WordTileView>();
            tile.Text = text;
            tile.Position = position;
            return tile;
        }
    }
}
