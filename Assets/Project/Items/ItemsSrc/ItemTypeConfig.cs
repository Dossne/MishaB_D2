using UnityEngine;

namespace VacuumSorter.Items
{
    [CreateAssetMenu(fileName = "ItemType", menuName = "VacuumSorter/Items/Item Type")]
    public sealed class ItemTypeConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _typeId = "item_type";

        [Header("Visual")]
        [SerializeField] private PrimitiveType _primitiveType = PrimitiveType.Cube;
        [SerializeField] private Vector3 _visualScale = new Vector3(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color _baseColor = Color.white;

        [Header("Physics")]
        [SerializeField, Min(0.05f)] private float _mass = 0.22f;
        [SerializeField, Min(0f)] private float _linearDamping = 0.6f;
        [SerializeField, Min(0f)] private float _angularDamping = 0.9f;

        public string TypeId => _typeId;
        public PrimitiveType PrimitiveType => _primitiveType;
        public Vector3 VisualScale => _visualScale;
        public Color BaseColor => _baseColor;
        public float Mass => _mass;
        public float LinearDamping => _linearDamping;
        public float AngularDamping => _angularDamping;
    }
}