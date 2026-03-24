using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VacuumSorter.MainUI
{
    [DisallowMultipleComponent]
    public sealed class MainUiProvider : MonoBehaviour
    {
        [Header("Optional bangers TMP font asset")]
        [SerializeField] private TMP_FontAsset _bangersCyrillicFontAsset;

        [Header("Optional bangers source font")]
        [SerializeField] private Font _bangersCyrillicFontSource;

        [Header("UI roots")]
        [SerializeField] private RectTransform _floatingTextParent;
        [SerializeField] private RectTransform _hudParent;
        [SerializeField] private RectTransform _popupParent;

        [Header("HUD labels")]
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _stateLabel;

        private TMP_FontAsset _runtimeGeneratedFontAsset;

        public RectTransform FloatingTextParent => _floatingTextParent;
        public RectTransform HudParent => _hudParent;
        public RectTransform PopupParent => _popupParent;

        public TextMeshProUGUI ScoreLabel => _scoreLabel;
        public TextMeshProUGUI LevelLabel => _levelLabel;
        public TextMeshProUGUI StateLabel => _stateLabel;

        private void Awake()
        {
            ConfigureCanvas();
            EnsureUiRoots();
            EnsureHudLabels();
            ApplyStageFont();
        }

        private void OnDestroy()
        {
            if (_runtimeGeneratedFontAsset != null)
            {
                Destroy(_runtimeGeneratedFontAsset);
                _runtimeGeneratedFontAsset = null;
            }
        }

        private void ConfigureCanvas()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureUiRoots()
        {
            _floatingTextParent = EnsureRoot(_floatingTextParent, "FloatingTextParent");
            _hudParent = EnsureRoot(_hudParent, "HudParent");
            _popupParent = EnsureRoot(_popupParent, "PopupParent");
        }

        private void EnsureHudLabels()
        {
            _scoreLabel = EnsureHudLabel(_scoreLabel, "ScoreLabel", "Score: 0", new Vector2(32f, -32f));
            _levelLabel = EnsureHudLabel(_levelLabel, "LevelLabel", "Level: 1", new Vector2(32f, -92f));
            _stateLabel = EnsureHudLabel(_stateLabel, "StateLabel", "State: Ready", new Vector2(32f, -152f));
        }

        private RectTransform EnsureRoot(RectTransform currentRoot, string rootName)
        {
            if (currentRoot != null)
            {
                return currentRoot;
            }

            var rootObject = new GameObject(rootName, typeof(RectTransform));
            var rootTransform = rootObject.GetComponent<RectTransform>();
            rootTransform.SetParent(transform, false);
            StretchToFull(rootTransform);
            return rootTransform;
        }

        private TextMeshProUGUI EnsureHudLabel(
            TextMeshProUGUI currentLabel,
            string name,
            string value,
            Vector2 anchoredPosition)
        {
            if (currentLabel != null)
            {
                return currentLabel;
            }

            var textObject = new GameObject(name, typeof(RectTransform));
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_hudParent, false);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(700f, 52f);

            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = value;
            label.fontSize = 48f;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Left;

            return label;
        }

        private void ApplyStageFont()
        {
            TMP_FontAsset stageFontAsset = _bangersCyrillicFontAsset;

            if (stageFontAsset == null
                && _bangersCyrillicFontSource != null
                && TMP_Settings.instance != null)
            {
                _runtimeGeneratedFontAsset = TMP_FontAsset.CreateFontAsset(_bangersCyrillicFontSource);
                stageFontAsset = _runtimeGeneratedFontAsset;
            }

            if (stageFontAsset == null)
            {
                return;
            }

            _scoreLabel.font = stageFontAsset;
            _levelLabel.font = stageFontAsset;
            _stateLabel.font = stageFontAsset;
        }

        private static void StretchToFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
