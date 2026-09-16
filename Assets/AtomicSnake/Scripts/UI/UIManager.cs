using System;
using System.Collections.Generic;
using System.Text;
using AtomicSnake.Gameplay;
using AtomicSnake.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AtomicSnake.UI
{
    // Builds and drives every screen (main menu, level select, HUD, popups)
    // procedurally with legacy uGUI, so no imported fonts/sprites are needed.
    public class UIManager : MonoBehaviour
    {
        public event Action OnPlayClicked;
        public event Action<int> OnLevelSelected;
        public event Action OnBackToMenu;
        public event Action OnLevelCompleteContinue;

        static readonly Color PanelBrown = new Color(0.42f, 0.29f, 0.16f, 0.95f);
        static readonly Color ParchmentColor = new Color(0.94f, 0.90f, 0.80f, 1f);
        static readonly Color TextDark = new Color(0.30f, 0.18f, 0.08f);
        static readonly Color ButtonGreen = new Color(0.32f, 0.55f, 0.33f);
        static readonly Color ButtonLocked = new Color(0.45f, 0.45f, 0.45f);

        RectTransform _mainMenuPanel;
        RectTransform _levelSelectPanel;
        RectTransform _hudPanel;
        RectTransform _levelCompletePanel;
        RectTransform _messageOverlay;

        Text _hudLevelText;
        Text _hudScoreText;
        Text _hudTimerText;
        Text _hudTrayText;
        Text _messageText;
        Text _levelCompleteText;
        readonly List<Button> _levelButtons = new();
        readonly List<Text> _levelButtonLabels = new();

        public void BuildUi()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            var root = canvas.transform;
            _mainMenuPanel = BuildMainMenu(root);
            _levelSelectPanel = BuildLevelSelect(root);
            _hudPanel = BuildHud(root);
            _levelCompletePanel = BuildLevelComplete(root);
            _messageOverlay = BuildMessageOverlay(root);

            HideAllPanels();
        }

        void HideAllPanels()
        {
            _mainMenuPanel.gameObject.SetActive(false);
            _levelSelectPanel.gameObject.SetActive(false);
            _hudPanel.gameObject.SetActive(false);
            _levelCompletePanel.gameObject.SetActive(false);
            _messageOverlay.gameObject.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            _mainMenuPanel.gameObject.SetActive(true);
        }

        public void ShowLevelSelect()
        {
            HideAllPanels();
            RefreshLevelButtons();
            _levelSelectPanel.gameObject.SetActive(true);
        }

        public void ShowHud()
        {
            HideAllPanels();
            _hudPanel.gameObject.SetActive(true);
        }

        public void ShowLevelComplete(string levelName, int score)
        {
            HideAllPanels();
            _levelCompleteText.text = $"{levelName} complete!\nScore: {score}";
            _levelCompletePanel.gameObject.SetActive(true);
        }

        public void ShowMessage(string message)
        {
            _messageText.text = message;
            _messageOverlay.gameObject.SetActive(true);
        }

        public void HideMessage()
        {
            _messageOverlay.gameObject.SetActive(false);
        }

        public void UpdateHud(string levelName, int questionNumber, int questionCount, int atomicNumber, int score, float timeRemaining)
        {
            _hudLevelText.text = $"{levelName} — Q{questionNumber}/{questionCount}\n#{atomicNumber}";
            _hudScoreText.text = $"Score: {score}";
            UpdateTimer(timeRemaining);
        }

        public void UpdateScore(int score)
        {
            _hudScoreText.text = $"Score: {score}";
        }

        public void UpdateTimer(float timeRemaining)
        {
            _hudTimerText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
        }

        public void UpdateCollectedTray(List<string> sequence, int pointer)
        {
            var sb = new StringBuilder("Collected: ");
            for (int i = 0; i < sequence.Count; i++)
            {
                if (i > 0) sb.Append(i == pointer ? "  " : " - ");
                sb.Append(i < pointer ? sequence[i] : "?");
            }
            _hudTrayText.text = sb.ToString();
        }

        void RefreshLevelButtons()
        {
            for (int i = 0; i < LevelCatalog.All.Length; i++)
            {
                var level = LevelCatalog.All[i];
                bool unlocked = GameProgress.IsLevelUnlocked(i);
                bool cleared = GameProgress.IsLevelCleared(i);
                string status = cleared ? "Cleared" : unlocked ? "Not played yet" : "Locked";
                _levelButtonLabels[i].text = $"{level.DisplayName}\n{level.ElementCount} elements to grow\n{status}";
                _levelButtons[i].interactable = unlocked;
                _levelButtons[i].image.color = unlocked ? ButtonGreen : ButtonLocked;
            }
        }

        #region Screen builders

        RectTransform BuildMainMenu(Transform parent)
        {
            var panel = CreatePanel(parent, "MainMenuPanel", new Color(0, 0, 0, 0));
            CreateText(panel, "Title", "Atomic Snake", 64, new Vector2(0, 120), new Vector2(900, 100), TextAnchor.MiddleCenter, Color.white);
            CreateText(panel, "Subtitle", "Grow your snake. Name the elements.", 28, new Vector2(0, 40), new Vector2(900, 60), TextAnchor.MiddleCenter, Color.white);
            CreateButton(panel, "PlayButton", "Play", new Vector2(0, -60), new Vector2(260, 80), ButtonGreen, () => OnPlayClicked?.Invoke());
            return panel;
        }

        RectTransform BuildLevelSelect(Transform parent)
        {
            var panel = CreatePanel(parent, "LevelSelectPanel", PanelBrown);
            CreateText(panel, "Title", "Choose a Garden Plot", 44, new Vector2(0, 260), new Vector2(900, 70), TextAnchor.MiddleCenter, Color.white);
            CreateText(panel, "Subtitle", "Pick a plot to grow that level's elements.", 22, new Vector2(0, 210), new Vector2(900, 40), TextAnchor.MiddleCenter, Color.white);

            for (int i = 0; i < LevelCatalog.All.Length; i++)
            {
                int levelIndex = i;
                float y = 100 - i * 130;
                var (button, label) = CreateLevelButton(panel, new Vector2(0, y), new Vector2(560, 100), () => OnLevelSelected?.Invoke(levelIndex));
                _levelButtons.Add(button);
                _levelButtonLabels.Add(label);
            }

            CreateButton(panel, "BackButton", "Back", new Vector2(0, -260), new Vector2(220, 70), ButtonLocked, () => OnBackToMenu?.Invoke());
            return panel;
        }

        RectTransform BuildHud(Transform parent)
        {
            var panel = CreatePanel(parent, "HudPanel", new Color(0, 0, 0, 0));

            var topLeft = CreateText(panel, "LevelText", "", 22, Vector2.zero, new Vector2(360, 90), TextAnchor.UpperLeft, Color.white);
            Anchor(topLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190, -55));
            _hudLevelText = topLeft.GetComponent<Text>();

            var score = CreateText(panel, "ScoreText", "Score: 0", 22, Vector2.zero, new Vector2(220, 40), TextAnchor.UpperLeft, Color.white);
            Anchor(score, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120, -95));
            _hudScoreText = score.GetComponent<Text>();

            var tray = CreateText(panel, "TrayText", "Collected:", 22, Vector2.zero, new Vector2(520, 50), TextAnchor.MiddleCenter, Color.white);
            Anchor(tray, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -35));
            _hudTrayText = tray.GetComponent<Text>();

            var timer = CreateText(panel, "TimerText", "", 26, Vector2.zero, new Vector2(140, 50), TextAnchor.UpperRight, Color.white);
            Anchor(timer, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-90, -35));
            _hudTimerText = timer.GetComponent<Text>();

            return panel;
        }

        RectTransform BuildLevelComplete(Transform parent)
        {
            var panel = CreatePanel(parent, "LevelCompletePanel", PanelBrown);
            var text = CreateText(panel, "CompleteText", "", 36, new Vector2(0, 60), new Vector2(800, 160), TextAnchor.MiddleCenter, Color.white);
            _levelCompleteText = text.GetComponent<Text>();
            CreateButton(panel, "ContinueButton", "Continue", new Vector2(0, -80), new Vector2(260, 80), ButtonGreen, () => OnLevelCompleteContinue?.Invoke());
            return panel;
        }

        RectTransform BuildMessageOverlay(Transform parent)
        {
            var panel = CreatePanel(parent, "MessageOverlay", new Color(0, 0, 0, 0.55f));
            var text = CreateText(panel, "MessageText", "", 40, Vector2.zero, new Vector2(900, 120), TextAnchor.MiddleCenter, Color.white);
            _messageText = text.GetComponent<Text>();
            return panel;
        }

        #endregion

        #region Widget helpers

        RectTransform CreatePanel(Transform parent, string name, Color background)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = background;
            return rect;
        }

        RectTransform CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = UiFontUtil.Default;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return rect;
        }

        void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
        }

        Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.color = color;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            CreateText(rect, "Label", label, 26, Vector2.zero, size, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        (Button, Text) CreateLevelButton(Transform parent, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("LevelButton", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.color = ButtonGreen;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var labelRect = CreateText(rect, "Label", "", 22, Vector2.zero, size, TextAnchor.MiddleCenter, Color.white);
            return (button, labelRect.GetComponent<Text>());
        }

        #endregion
    }
}
