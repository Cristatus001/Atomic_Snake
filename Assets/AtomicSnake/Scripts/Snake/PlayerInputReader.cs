using UnityEngine;

namespace AtomicSnake.Snake
{
    // Arrow keys / WASD pick a cardinal target heading; SnakeController turns
    // smoothly toward it instead of snapping, so corners curve naturally.
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
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                _snake.SetTargetHeading(0f);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                _snake.SetTargetHeading(90f);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                _snake.SetTargetHeading(180f);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                _snake.SetTargetHeading(270f);
        }
    }
}
