using AtomicSnake.Field;
using AtomicSnake.Gameplay;
using AtomicSnake.Snake;
using AtomicSnake.UI;
using UnityEngine;

namespace AtomicSnake.Bootstrap
{
    // Wires the pre-built scene together and drives the menu -> level select
    // -> play flow. Every visual (Canvas screens, snake, field, tiles) is
    // authored as a real scene object or prefab, assigned below — this
    // script only calls Initialize and subscribes to events, it never
    // creates GameObjects or components itself.
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameObject fieldRoot;
        [SerializeField] SnakeController snake;
        [SerializeField] GameplayController gameplay;
        [SerializeField] UIManager ui;
        [SerializeField] WordTileView wordTilePrefab;

        void Awake()
        {
            snake.Initialize(fieldRoot.transform);
            gameplay.Initialize(snake, fieldRoot.transform, ui, wordTilePrefab);
            fieldRoot.SetActive(false);

            ui.OnPlayClicked += () => ui.ShowLevelSelect();
            ui.OnBackToMenu += () => ui.ShowMainMenu();
            ui.OnLevelSelected += StartLevel;
            ui.OnLevelCompleteContinue += ReturnToLevelSelect;

            ui.ShowMainMenu();
        }

        void StartLevel(int levelIndex)
        {
            fieldRoot.SetActive(true);
            gameplay.StartLevel(levelIndex);
        }

        void ReturnToLevelSelect()
        {
            fieldRoot.SetActive(false);
            gameplay.StopLevel();
            ui.ShowLevelSelect();
        }
    }
}
