using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.Field
{
    // A single collectible word-part tile (e.g. "un", "nil", "ium") floating
    // at one point in the continuous field.
    public class WordTileView : MonoBehaviour
    {
        static readonly Color TileColor = new Color(0.86f, 0.55f, 0.70f);

        public string Text { get; private set; }
        public Vector2 Position { get; private set; }

        public static WordTileView Create(Transform parent, string text, Vector2 position)
        {
            var go = new GameObject($"Tile_{text}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.GetCircle(TileColor);
            sr.sortingOrder = 5;
            SpriteFactory.SetWorldSize(sr, GridConfig.CellSize * 0.88f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            var mesh = labelGo.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.08f;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            var renderer = labelGo.GetComponent<MeshRenderer>();
            renderer.sortingOrder = 6;

            var tile = go.AddComponent<WordTileView>();
            tile.Text = text;
            tile.Position = position;
            return tile;
        }
    }
}
