using UnityEngine;

namespace VacuumSorter.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class ItemView : MonoBehaviour
    {
        [SerializeField] private ItemTypeConfig _itemType;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Rigidbody _rigidbody;
        private Renderer _cachedRenderer;
        private MaterialPropertyBlock _propertyBlock;

        public ItemTypeConfig ItemType => _itemType;

        public void Initialize(ItemTypeConfig itemType)
        {
            _itemType = itemType;
            ApplyType();
        }

        private void Awake()
        {
            ApplyType();
        }

        private void ApplyType()
        {
            if (_itemType == null)
            {
                return;
            }

            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.mass = _itemType.Mass;
            _rigidbody.linearDamping = _itemType.LinearDamping;
            _rigidbody.angularDamping = _itemType.AngularDamping;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.solverIterations = 10;
            _rigidbody.solverVelocityIterations = 10;

            transform.localScale = _itemType.VisualScale;
            gameObject.name = string.IsNullOrWhiteSpace(_itemType.TypeId) ? "Item" : _itemType.TypeId;

            if (_cachedRenderer == null)
            {
                _cachedRenderer = GetComponentInChildren<Renderer>();
            }

            if (_cachedRenderer == null)
            {
                return;
            }

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            _cachedRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, _itemType.BaseColor);
            _propertyBlock.SetColor(ColorId, _itemType.BaseColor);
            _cachedRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}