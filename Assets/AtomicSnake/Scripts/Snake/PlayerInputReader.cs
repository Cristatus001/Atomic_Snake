using UnityEngine;

namespace AtomicSnake.Snake
{
    // Arrow keys / WASD steering for the snake.
    [RequireComponent(typeof(SnakeController))]
    public class PlayerInputReader : MonoBehaviour
    {
        SnakeController _snake;

        void Awake()
        {
            _snake = GetComponent<SnakeController>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                _snake.TrySetDirection(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                _snake.TrySetDirection(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                _snake.TrySetDirection(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                _snake.TrySetDirection(Vector2Int.right);
        }
    }
}
