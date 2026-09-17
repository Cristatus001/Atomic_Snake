using UnityEngine;

namespace AtomicSnake.Field
{
    // A static obstacle the snake must avoid. Sprite/color presentation lives
    // on the Obstacle prefab, so an artist can restyle it without touching code.
    public class ObstacleView : MonoBehaviour
    {
        public Vector2 Position { get; private set; }

        public static ObstacleView Create(ObstacleView prefab, Transform parent, Vector2 position)
        {
            var instance = Instantiate(prefab, parent);
            instance.transform.localPosition = position;
            instance.Position = position;
            return instance;
        }
    }
}
