using UnityEngine;

namespace AtomicSnake.Field
{
    // Data + positioning for one collectible word-part tile. All presentation
    // (sprite, color, font) lives on the WordTile prefab's SpriteHolder and
    // TextHolder children, so an artist can restyle it without touching code.
    public class WordTileView : MonoBehaviour
    {
        [SerializeField] TextMesh textHolder;

        public string Text { get; private set; }
        public Vector2 Position { get; private set; }

        public static WordTileView Create(WordTileView prefab, Transform parent, string text, Vector2 position)
        {
            var instance = Instantiate(prefab, parent);
            instance.transform.localPosition = position;
            instance.Text = text;
            instance.Position = position;
            if (instance.textHolder != null)
                instance.textHolder.text = text;
            return instance;
        }
    }
}
