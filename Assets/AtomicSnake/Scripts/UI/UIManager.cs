using System;
using System.Collections.Generic;
using System.Text;
using AtomicSnake.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace AtomicSnake.UI
{
    // Thin façade over a pre-built Canvas: only toggles screen visibility and
    // sets text/label content from gameplay data. All layout, fonts, colors
    // and button art are authored on the Canvas itself, so an artist can
    // restyle any screen without touching this script.
    public class UIManager : MonoBehaviour
    {
        public event Action OnPlayClicked;
        public event Action<int> OnLevelSelected;
        public event Action OnBackToMenu;
        public event Action OnLevelCompleteContinue;
        public event Action OnHowToPlayClicked;
        public event Action OnInstructionsContinue;

        [Header("Screens")]
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] GameObject levelSelectPanel;
        [SerializeField] GameObject hudPanel;
        [SerializeField] GameObject levelCompletePanel;
        [SerializeField] GameObject messageOverlay;
        [SerializeField] GameObject instructionsPanel;
        [SerializeField] GameObject loadingPanel;

        [Header("Main Menu")]
        [SerializeField] Button playButton;

        [Header("Level Select")]
        [SerializeField] Button[] levelButtons;
        [SerializeField] Text[] levelButtonLabels;
        [SerializeField] Button backButton;
        [SerializeField] Button howToPlayButton;

        [Header("HUD")]
        [SerializeField] Text hudLevelText;
        [SerializeField] Text hudScoreText;
        [SerializeField] Text hudTrayText;
        [SerializeField] Text hudTimerText;
        [SerializeField] MiniMapView miniMap;

        [Header("Level Complete")]
        [SerializeField] Text levelCompleteText;
        [SerializeField] Button continueButton;

        [Header("Message")]
        [SerializeField] Text messageText;

        [Header("Instructions")]
        [SerializeField] Button instructionsContinueButton;

        [Header("Loading")]
        [SerializeField] Text loadingFactText;

        public MiniMapView MiniMap => miniMap;

        void Awake()
        {
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            backButton.onClick.AddListener(() => OnBackToMenu?.Invoke());
            continueButton.onClick.AddListener(() => OnLevelCompleteContinue?.Invoke());
            howToPlayButton.onClick.AddListener(() => OnHowToPlayClicked?.Invoke());
            instructionsContinueButton.onClick.AddListener(() => OnInstructionsContinue?.Invoke());

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i;
                levelButtons[i].onClick.AddListener(() => OnLevelSelected?.Invoke(levelIndex));
            }

            HideAllPanels();
        }

        void HideAllPanels()
        {
            mainMenuPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            hudPanel.SetActive(false);
            levelCompletePanel.SetActive(false);
            messageOverlay.SetActive(false);
            instructionsPanel.SetActive(false);
            loadingPanel.SetActive(false);
            miniMap.Hide();
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            mainMenuPanel.SetActive(true);
        }

        public void ShowLevelSelect()
        {
            HideAllPanels();
            RefreshLevelButtons();
            levelSelectPanel.SetActive(true);
        }

        public void ShowHud()
        {
            HideAllPanels();
            hudPanel.SetActive(true);
        }

        public void ShowLevelComplete(string levelName, int score)
        {
            HideAllPanels();
            levelCompleteText.text = $"{levelName} complete!\nScore: {score}";
            levelCompletePanel.SetActive(true);
        }

        public void ShowInstructions()
        {
            HideAllPanels();
            instructionsPanel.SetActive(true);
        }

        public void ShowLoading(string fact)
        {
            HideAllPanels();
            loadingFactText.text = fact;
            loadingPanel.SetActive(true);
        }

        public void ShowMessage(string message)
        {
            messageText.text = message;
            messageOverlay.SetActive(true);
        }

        public void HideMessage()
        {
            messageOverlay.SetActive(false);
        }

        public void UpdateHud(string levelName, int questionNumber, int questionCount, int atomicNumber, int score, float timeRemaining)
        {
            hudLevelText.text = $"{levelName} — Q{questionNumber}/{questionCount}\n#{atomicNumber}";
            hudScoreText.text = $"Score: {score}";
            UpdateTimer(timeRemaining);
        }

        public void UpdateScore(int score)
        {
            hudScoreText.text = $"Score: {score}";
        }

        public void UpdateTimer(float timeRemaining)
        {
            hudTimerText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
        }

        public void UpdateCollectedTray(List<string> sequence, int pointer)
        {
            var sb = new StringBuilder("Collected: ");
            for (int i = 0; i < sequence.Count; i++)
            {
                if (i > 0) sb.Append(i == pointer ? "  " : " - ");
                sb.Append(i < pointer ? sequence[i] : "?");
            }
            hudTrayText.text = sb.ToString();
        }

        void RefreshLevelButtons()
        {
            for (int i = 0; i < LevelCatalog.All.Length; i++)
            {
                var level = LevelCatalog.All[i];
                bool unlocked = GameProgress.IsLevelUnlocked(i);
                bool cleared = GameProgress.IsLevelCleared(i);
                string status = cleared ? "Cleared" : unlocked ? "Not played yet" : "Locked";
                levelButtonLabels[i].text = $"{level.DisplayName}\n{level.ElementCount} elements to grow\n{status}";
                levelButtons[i].interactable = unlocked;
            }
        }
    }
}
