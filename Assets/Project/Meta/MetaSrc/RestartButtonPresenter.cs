using System;
using TMPro;
using VacuumSorter.MainUI;
using UnityEngine;
using UnityEngine.UI;

namespace VacuumSorter.Meta
{
    [DisallowMultipleComponent]
    public sealed class RestartButtonPresenter : MonoBehaviour
    {
        private MainUiProvider _mainUiProvider;
        private RectTransform _popupRoot;
        private RectTransform _panelRoot;
        private TextMeshProUGUI _titleLabel;
        private TextMeshProUGUI _bodyLabel;
        private Button _restartButton;
        private Button _nextButton;

        public void Initialize(MainUiProvider mainUiProvider)
        {
            if (mainUiProvider == null)
            {
                return;
            }

            _mainUiProvider = mainUiProvider;
            EnsureView();
        }

        public void ShowCompletion(int levelNumber, Action onRestart, Action onNext)
        {
            EnsureView();
            if (_popupRoot == null)
            {
                return;
            }

            _popupRoot.gameObject.SetActive(true);

            if (_titleLabel != null)
            {
                _titleLabel.text = "Level Complete";
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.text = $"Level {levelNumber} cleared.\nNext level is a placeholder reload.";
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            }

            if (_nextButton != null)
            {
                _nextButton.onClick.RemoveAllListeners();
                _nextButton.onClick.AddListener(() => onNext?.Invoke());
            }
        }

        public void Hide()
        {
            if (_popupRoot != null)
            {
                _popupRoot.gameObject.SetActive(false);
            }
        }

        private void EnsureView()
        {
            if (_mainUiProvider == null || _mainUiProvider.PopupParent == null)
            {
                return;
            }

            if (_popupRoot == null)
            {
                var popupRootObject = new GameObject("RoundCompletionPopup", typeof(RectTransform), typeof(Image));
                _popupRoot = popupRootObject.GetComponent<RectTransform>();
                _popupRoot.SetParent(_mainUiProvider.PopupParent, false);
                Stretch(_popupRoot);

                var dimmerImage = popupRootObject.GetComponent<Image>();
                dimmerImage.color = new Color(0f, 0f, 0f, 0.55f);

                var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
                _panelRoot = panelObject.GetComponent<RectTransform>();
                _panelRoot.SetParent(_popupRoot, false);
                _panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
                _panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
                _panelRoot.pivot = new Vector2(0.5f, 0.5f);
                _panelRoot.sizeDelta = new Vector2(930f, 520f);

                var panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.09f, 0.14f, 0.2f, 0.95f);

                _titleLabel = CreateText("Title", _panelRoot, new Vector2(0f, 160f), new Vector2(860f, 100f), 72f, TextAlignmentOptions.Center);
                _bodyLabel = CreateText("Body", _panelRoot, new Vector2(0f, 42f), new Vector2(860f, 140f), 42f, TextAlignmentOptions.Center);

                _restartButton = CreateButton("RestartButton", _panelRoot, new Vector2(-180f, -150f), "Restart", new Color(0.18f, 0.43f, 0.22f, 1f));
                _nextButton = CreateButton("NextButton", _panelRoot, new Vector2(180f, -150f), "Next", new Color(0.2f, 0.32f, 0.62f, 1f));
            }

            ApplySharedFont();
        }

        private Button CreateButton(string objectName, RectTransform parent, Vector2 anchoredPosition, string labelText, Color color)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = new Vector2(300f, 120f);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = color;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;

            var label = CreateText("Label", buttonRect, Vector2.zero, buttonRect.sizeDelta, 46f, TextAlignmentOptions.Center);
            label.text = labelText;
            label.color = Color.white;

            return button;
        }

        private TextMeshProUGUI CreateText(
            string objectName,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            float fontSize,
            TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(parent, false);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = anchoredPosition;
            textRect.sizeDelta = size;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            return text;
        }

        private void ApplySharedFont()
        {
            var stageFont = _mainUiProvider != null && _mainUiProvider.ScoreLabel != null
                ? _mainUiProvider.ScoreLabel.font
                : null;

            if (stageFont == null)
            {
                return;
            }

            if (_titleLabel != null)
            {
                _titleLabel.font = stageFont;
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.font = stageFont;
            }

            ApplyFontToButton(_restartButton, stageFont);
            ApplyFontToButton(_nextButton, stageFont);
        }

        private static void ApplyFontToButton(Button button, TMP_FontAsset fontAsset)
        {
            if (button == null)
            {
                return;
            }

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.font = fontAsset;
            }
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
