using VacuumSorter.PlayerInput;
using UnityEngine;

namespace VacuumSorter.Robot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RobotController : MonoBehaviour
    {
        private const float VisualY = 0.35f;

        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private RobotConfig _config;

        private Rigidbody _rigidbody;
        private bool _isInitialized;

        public void Initialize(PlayerInputReader inputReader, RobotConfig config)
        {
            _inputReader = inputReader;
            _config = config;
            SetupPhysics();
            EnsureRobotVisuals();
            _isInitialized = _inputReader != null && _config != null;
        }

        private void Awake()
        {
            SetupPhysics();
            EnsureRobotVisuals();
        }

        private void FixedUpdate()
        {
            if (!_isInitialized || _inputReader == null || _config == null)
            {
                return;
            }

            var rawInput = _inputReader.MoveInput;
            var inputMagnitude = Mathf.Clamp01(rawInput.magnitude);
            var shapedMagnitude = Mathf.Pow(inputMagnitude, _config.InputExponent);

            Vector3 desiredDirection = Vector3.zero;
            if (inputMagnitude > 0.001f)
            {
                desiredDirection = new Vector3(rawInput.x, 0f, rawInput.y).normalized;
            }

            var targetSpeed = shapedMagnitude * _config.MaxSpeed;
            var targetVelocity = desiredDirection * targetSpeed;

            var currentVelocity = _rigidbody.linearVelocity;
            var currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            var acceleration = inputMagnitude > 0.001f ? _config.Acceleration : _config.Deceleration;
            var maxDelta = acceleration * Time.fixedDeltaTime;
            var nextPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, maxDelta);

            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, 0f, nextPlanarVelocity.z);

            if (nextPlanarVelocity.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(nextPlanarVelocity.normalized, Vector3.up);
                var rotationStep = _config.TurnSpeed * Time.fixedDeltaTime;
                _rigidbody.MoveRotation(Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, rotationStep));
            }

            ClampInsideArena();
        }

        private void SetupPhysics()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.mass = 2.5f;
            _rigidbody.linearDamping = 5f;
            _rigidbody.angularDamping = 20f;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var bodyCollider = GetComponent<CapsuleCollider>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            bodyCollider.radius = 0.62f;
            bodyCollider.height = 0.72f;
            bodyCollider.center = new Vector3(0f, VisualY, -0.1f);
            bodyCollider.direction = 1;
        }

        private void ClampInsideArena()
        {
            var currentPosition = _rigidbody.position;
            var radius = _config.RobotRadius;
            var clampedX = Mathf.Clamp(currentPosition.x, -_config.ArenaHalfWidth + radius, _config.ArenaHalfWidth - radius);
            var clampedZ = Mathf.Clamp(currentPosition.z, -_config.ArenaHalfDepth + radius, _config.ArenaHalfDepth - radius);

            var clampedPosition = new Vector3(clampedX, VisualY, clampedZ);
            if ((clampedPosition - currentPosition).sqrMagnitude < 0.000001f)
            {
                return;
            }

            _rigidbody.position = clampedPosition;

            var velocity = _rigidbody.linearVelocity;
            if (Mathf.Abs(currentPosition.x - clampedX) > 0.0001f)
            {
                velocity.x = 0f;
            }

            if (Mathf.Abs(currentPosition.z - clampedZ) > 0.0001f)
            {
                velocity.z = 0f;
            }

            _rigidbody.linearVelocity = velocity;
        }

        private void EnsureRobotVisuals()
        {
            if (transform.Find("RobotBody") != null)
            {
                return;
            }

            var body = CreateVisualCube("RobotBody", transform, new Vector3(0f, VisualY, -0.05f), new Vector3(1.3f, 0.55f, 1.45f), new Color(0.72f, 0.79f, 0.86f, 1f));
            var top = CreateVisualCube("RobotTop", transform, new Vector3(0f, 0.66f, -0.08f), new Vector3(0.8f, 0.2f, 0.8f), new Color(0.18f, 0.22f, 0.27f, 1f));
            CreateVisualCube("ScoopLeft", transform, new Vector3(-0.56f, 0.25f, 0.74f), new Vector3(0.18f, 0.5f, 1.2f), new Color(0.97f, 0.78f, 0.32f, 1f));
            CreateVisualCube("ScoopRight", transform, new Vector3(0.56f, 0.25f, 0.74f), new Vector3(0.18f, 0.5f, 1.2f), new Color(0.97f, 0.78f, 0.32f, 1f));
            CreateVisualCube("ScoopBack", transform, new Vector3(0f, 0.25f, 0.17f), new Vector3(1.12f, 0.5f, 0.18f), new Color(0.91f, 0.66f, 0.24f, 1f));

            body.transform.SetSiblingIndex(0);
            top.transform.SetSiblingIndex(1);

            EnsureScoopCollider("ScoopColliderLeft", new Vector3(-0.56f, 0.25f, 0.74f), new Vector3(0.18f, 0.45f, 1.2f));
            EnsureScoopCollider("ScoopColliderRight", new Vector3(0.56f, 0.25f, 0.74f), new Vector3(0.18f, 0.45f, 1.2f));
            EnsureScoopCollider("ScoopColliderBack", new Vector3(0f, 0.25f, 0.17f), new Vector3(1.12f, 0.45f, 0.18f));
        }

        private static GameObject CreateVisualCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;

            var collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(propertyBlock);
            }

            return cube;
        }

        private void EnsureScoopCollider(string name, Vector3 localPosition, Vector3 size)
        {
            var existing = transform.Find(name);
            BoxCollider collider;

            if (existing == null)
            {
                var child = new GameObject(name);
                child.transform.SetParent(transform, false);
                child.transform.localPosition = localPosition;
                child.transform.localRotation = Quaternion.identity;
                collider = child.AddComponent<BoxCollider>();
            }
            else
            {
                existing.localPosition = localPosition;
                existing.localRotation = Quaternion.identity;
                collider = existing.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = existing.gameObject.AddComponent<BoxCollider>();
                }
            }

            collider.size = size;
        }
    }
}