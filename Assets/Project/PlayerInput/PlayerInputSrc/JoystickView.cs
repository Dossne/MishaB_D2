using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VacuumSorter.PlayerInput
{
    [DisallowMultipleComponent]
    public sealed class JoystickView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _baseRect;
        [SerializeField] private RectTransform _handleRect;
        [SerializeField, Min(40f)] private float _radius = 110f;

        private static Sprite s_fallbackSprite;

        private Vector2 _value;
        private Camera _uiCamera;

        public Vector2 Value => _value;

        private void Awake()
        {
            EnsureVisualHierarchy();
            ResetHandle();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _value = Vector2.zero;
            ResetHandle();
        }

        private void EnsureVisualHierarchy()
        {
            var rootRect = transform as RectTransform;
            if (rootRect == null)
            {
                rootRect = gameObject.AddComponent<RectTransform>();
            }

            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(0f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(320f, 320f);
            if (rootRect.anchoredPosition == Vector2.zero)
            {
                rootRect.anchoredPosition = new Vector2(180f, 180f);
            }

            _baseRect = EnsureImageRect(_baseRect, "Base", rootRect, new Vector2(240f, 240f), new Color(0f, 0f, 0f, 0.35f));
            _handleRect = EnsureImageRect(_handleRect, "Handle", _baseRect, new Vector2(110f, 110f), new Color(1f, 1f, 1f, 0.85f));

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _uiCamera = canvas.worldCamera;
            }
            else
            {
                _uiCamera = null;
            }
        }

        private static RectTransform EnsureImageRect(
            RectTransform current,
            string name,
            RectTransform parent,
            Vector2 size,
            Color color)
        {
            RectTransform rect;
            if (current != null)
            {
                rect = current;
            }
            else
            {
                var child = new GameObject(name, typeof(RectTransform), typeof(Image));
                rect = child.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            var image = rect.GetComponent<Image>();
            image.sprite = GetFallbackSprite();
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = true;

            return rect;
        }

        private static Sprite GetFallbackSprite()
        {
            if (s_fallbackSprite != null)
            {
                return s_fallbackSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);

            s_fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            s_fallbackSprite.name = "JoystickFallbackSprite";
            return s_fallbackSprite;
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (_baseRect == null)
            {
                return;
            }

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, eventData.position, _uiCamera, out localPoint))
            {
                return;
            }

            _value = Vector2.ClampMagnitude(localPoint / _radius, 1f);

            if (_handleRect != null)
            {
                _handleRect.anchoredPosition = _value * _radius;
            }
        }

        private void ResetHandle()
        {
            if (_handleRect != null)
            {
                _handleRect.anchoredPosition = Vector2.zero;
            }
        }
    }
}