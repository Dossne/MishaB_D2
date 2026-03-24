using System.Collections;
using TMPro;
using VacuumSorter.MainUI;
using UnityEngine;

namespace VacuumSorter.Feedback
{
    [DisallowMultipleComponent]
    public sealed class FloatingTextPresenter : MonoBehaviour
    {
        private const float OutlineWidth = 0.12f;

        private MainUiProvider _mainUiProvider;
        private FeedbackConfig _feedbackConfig;
        private RectTransform _floatingParent;
        private Camera _worldCamera;

        public void Initialize(MainUiProvider mainUiProvider, FeedbackConfig feedbackConfig)
        {
            _mainUiProvider = mainUiProvider;
            _feedbackConfig = feedbackConfig;
            _floatingParent = _mainUiProvider != null ? _mainUiProvider.FloatingTextParent : null;
            _worldCamera = Camera.main;
        }

        public void ShowWorldText(string value, Vector3 worldPosition, Color color)
        {
            if (_floatingParent == null || _feedbackConfig == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var textObject = new GameObject("FloatingText", typeof(RectTransform), typeof(TextMeshProUGUI));
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_floatingParent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(280f, 90f);

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.color = color;
            text.fontSize = _feedbackConfig.FloatingText.FontSize;
            text.alignment = TextAlignmentOptions.Center;

            if (_mainUiProvider != null && _mainUiProvider.ScoreLabel != null)
            {
                text.font = _mainUiProvider.ScoreLabel.font;
            }

            ConfigureOutline(text);

            StartCoroutine(AnimateFloatingText(rectTransform, text, worldPosition));
        }

        private static void ConfigureOutline(TextMeshProUGUI text)
        {
            if (text == null)
            {
                return;
            }

            var material = text.fontMaterial;
            if (material == null)
            {
                return;
            }

            material.EnableKeyword("OUTLINE_ON");

            if (material.HasProperty(ShaderUtilities.ID_OutlineColor))
            {
                material.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.8f));
            }

            if (material.HasProperty(ShaderUtilities.ID_OutlineWidth))
            {
                material.SetFloat(ShaderUtilities.ID_OutlineWidth, OutlineWidth);
            }

            text.fontMaterial = material;
            text.outlineColor = new Color(0f, 0f, 0f, 0.8f);
            text.outlineWidth = OutlineWidth;
        }

        private IEnumerator AnimateFloatingText(RectTransform rectTransform, TextMeshProUGUI text, Vector3 worldPosition)
        {
            var settings = _feedbackConfig.FloatingText;
            var duration = Mathf.Max(0.01f, settings.Lifetime);
            var elapsed = 0f;

            var baseLocalPosition = WorldToUiPosition(worldPosition);
            rectTransform.anchoredPosition = baseLocalPosition;
            rectTransform.localScale = Vector3.one * settings.StartScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 2.1f);

                var rise = new Vector2(0f, settings.RiseDistance * eased);
                rectTransform.anchoredPosition = baseLocalPosition + rise;
                rectTransform.localScale = Vector3.one * Mathf.Lerp(settings.StartScale, settings.EndScale, eased);

                var textColor = text.color;
                textColor.a = Mathf.Lerp(1f, 0f, eased);
                text.color = textColor;

                yield return null;
            }

            Destroy(rectTransform.gameObject);
        }

        private Vector2 WorldToUiPosition(Vector3 worldPosition)
        {
            Vector2 screenPoint;
            if (_worldCamera != null)
            {
                var projected = _worldCamera.WorldToScreenPoint(worldPosition);
                screenPoint = new Vector2(projected.x, projected.y);
            }
            else
            {
                screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _floatingParent,
                screenPoint,
                null,
                out var localPoint);

            return localPoint;
        }
    }
}
