using UnityEngine;

namespace AtomicSnake.Snake
{
    // Arrow keys/WASD, or a swipe/drag anywhere on the field (mouse or touch),
    // pick a cardinal target heading; SnakeController turns smoothly toward
    // it instead of snapping, so corners curve naturally.
    [RequireComponent(typeof(SnakeController))]
    public class PlayerInputReader : MonoBehaviour
    {
        const float SwipeThresholdPixels = 40f;

        SnakeController _snake;
        bool _dragging;
        Vector2 _dragStart;

        void Awake()
        {
            _snake = GetComponent<SnakeController>();
        }

        void Update()
        {
            ReadKeyboard();
            ReadSwipe();
        }

        void ReadKeyboard()
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

        void ReadSwipe()
        {
            Vector2 pointerPos;
            bool pressed, held, released;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                pointerPos = touch.position;
                pressed = touch.phase == TouchPhase.Began;
                held = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                released = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            else
            {
                pointerPos = Input.mousePosition;
                pressed = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
                released = Input.GetMouseButtonUp(0);
            }

            if (pressed)
            {
                _dragging = true;
                _dragStart = pointerPos;
            }
            else if (_dragging && held)
            {
                var delta = pointerPos - _dragStart;
                if (delta.magnitude >= SwipeThresholdPixels)
                {
                    _snake.SetTargetHeading(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                        ? (delta.x > 0f ? 0f : 180f)
                        : (delta.y > 0f ? 90f : 270f));
                    _dragStart = pointerPos;
                }
            }
            else if (released)
            {
                _dragging = false;
            }
        }
    }
}
