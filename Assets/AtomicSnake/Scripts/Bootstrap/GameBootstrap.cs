using AtomicSnake.Field;
using AtomicSnake.Gameplay;
using AtomicSnake.Runtime;
using AtomicSnake.Snake;
using AtomicSnake.UI;
using UnityEngine;

namespace AtomicSnake.Bootstrap
{
    // Single entry point: builds the camera, field, snake, UI and gameplay
    // controller at runtime, then wires the menu -> level select -> play flow.
    public class GameBootstrap : MonoBehaviour
    {
        GameObject _fieldRoot;
        GameplayController _gameplay;
        UIManager _ui;

        void Awake()
        {
            CameraRig.Setup();

            _fieldRoot = new GameObject("FieldRoot");
            FieldBackground.Build(_fieldRoot.transform);
            _fieldRoot.SetActive(false);

            var snakeGo = new GameObject("Snake");
            var snake = snakeGo.AddComponent<SnakeController>();
            snakeGo.AddComponent<PlayerInputReader>();
            snake.Initialize(_fieldRoot.transform);

            var uiGo = new GameObject("UIManager");
            _ui = uiGo.AddComponent<UIManager>();
            _ui.BuildUi();

            var gameplayGo = new GameObject("GameplayController");
            _gameplay = gameplayGo.AddComponent<GameplayController>();
            _gameplay.Initialize(snake, _fieldRoot.transform, _ui);

            _ui.OnPlayClicked += () => _ui.ShowLevelSelect();
            _ui.OnBackToMenu += () => _ui.ShowMainMenu();
            _ui.OnLevelSelected += StartLevel;
            _ui.OnLevelCompleteContinue += ReturnToLevelSelect;

            _ui.ShowMainMenu();
        }

        void StartLevel(int levelIndex)
        {
            _fieldRoot.SetActive(true);
            _gameplay.StartLevel(levelIndex);
        }

        void ReturnToLevelSelect()
        {
            _fieldRoot.SetActive(false);
            _gameplay.StopLevel();
            _ui.ShowLevelSelect();
        }
    }
}
