using System.Collections;
using AtomicSnake.Chemistry;
using AtomicSnake.Field;
using AtomicSnake.Gameplay;
using AtomicSnake.Snake;
using AtomicSnake.UI;
using UnityEngine;

namespace AtomicSnake.Bootstrap
{
    // Wires the pre-built scene together and drives the menu -> instructions
    // (first time only) -> loading -> level select -> play flow. Every
    // visual (Canvas screens, snake, field, tiles) is authored as a real
    // scene object or prefab, assigned below — this script only calls
    // Initialize and subscribes to events, it never creates GameObjects or
    // components itself.
    public class GameBootstrap : MonoBehaviour
    {
        const string HasSeenInstructionsKey = "AtomicSnake.HasSeenInstructions";
        const float LoadingDurationSeconds = 1.3f;

        [SerializeField] GameObject fieldRoot;
        [SerializeField] SnakeController snake;
        [SerializeField] GameplayController gameplay;
        [SerializeField] UIManager ui;
        [SerializeField] WordTileView wordTilePrefab;
        [SerializeField] PowerUpView mapPickupPrefab;
        [SerializeField] PowerUpView clockPickupPrefab;
        [SerializeField] ObstacleView obstaclePrefab;

        int _pendingLevelIndex;
        bool _instructionsForLevelStart;

        // Start (not Awake): this wiring calls into UIManager's own Awake-time
        // setup (HideAllPanels, button listeners), and Unity does not
        // guarantee Awake() order between different GameObjects — running
        // here guarantees UIManager.Awake() has already completed.
        void Start()
        {
            snake.Initialize(fieldRoot.transform);
            gameplay.Initialize(snake, fieldRoot.transform, ui, wordTilePrefab, mapPickupPrefab, clockPickupPrefab, obstaclePrefab);
            fieldRoot.SetActive(false);

            ui.OnPlayClicked += () => ui.ShowLevelSelect();
            ui.OnBackToMenu += () => ui.ShowMainMenu();
            ui.OnLevelSelected += RequestStartLevel;
            ui.OnLevelCompleteContinue += ReturnToLevelSelect;
            ui.OnHowToPlayClicked += ShowInstructionsManually;
            ui.OnInstructionsContinue += HandleInstructionsContinue;

            ui.ShowMainMenu();
        }

        void RequestStartLevel(int levelIndex)
        {
            _pendingLevelIndex = levelIndex;

            if (!PlayerPrefs.HasKey(HasSeenInstructionsKey))
            {
                _instructionsForLevelStart = true;
                ui.ShowInstructions();
                return;
            }

            StartCoroutine(LoadThenStartLevel(levelIndex));
        }

        void ShowInstructionsManually()
        {
            _instructionsForLevelStart = false;
            ui.ShowInstructions();
        }

        void HandleInstructionsContinue()
        {
            PlayerPrefs.SetInt(HasSeenInstructionsKey, 1);
            PlayerPrefs.Save();

            if (_instructionsForLevelStart)
                StartCoroutine(LoadThenStartLevel(_pendingLevelIndex));
            else
                ui.ShowLevelSelect();
        }

        IEnumerator LoadThenStartLevel(int levelIndex)
        {
            ui.ShowLoading(ChemistryFacts.GetRandom());
            yield return new WaitForSeconds(LoadingDurationSeconds);

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
