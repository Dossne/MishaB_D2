using UnityEngine;

namespace VacuumSorter.Robot
{
    [CreateAssetMenu(fileName = "RobotConfig", menuName = "VacuumSorter/Robot/Robot Config")]
    public sealed class RobotConfig : ScriptableObject
    {
        [Header("Robot asset")]
        [SerializeField] private GameObject _robotPrefab;

        [Header("Spawn")]
        [SerializeField] private Vector3 _startPosition = new Vector3(0f, 0.35f, -4f);

        [Header("Movement")]
        [SerializeField, Min(0.5f)] private float _maxSpeed = 7.5f;
        [SerializeField, Min(0.5f)] private float _acceleration = 18f;
        [SerializeField, Min(0.5f)] private float _deceleration = 28f;
        [SerializeField, Min(1f)] private float _turnSpeed = 540f;
        [SerializeField, Range(0.8f, 2.5f)] private float _inputExponent = 1.35f;

        [Header("Arena clamp")]
        [SerializeField, Min(2f)] private float _arenaHalfWidth = 8f;
        [SerializeField, Min(2f)] private float _arenaHalfDepth = 8f;
        [SerializeField, Min(0.1f)] private float _robotRadius = 0.85f;

        public GameObject RobotPrefab => _robotPrefab;
        public Vector3 StartPosition => _startPosition;
        public float MaxSpeed => _maxSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float TurnSpeed => _turnSpeed;
        public float InputExponent => _inputExponent;
        public float ArenaHalfWidth => _arenaHalfWidth;
        public float ArenaHalfDepth => _arenaHalfDepth;
        public float RobotRadius => _robotRadius;
    }
}