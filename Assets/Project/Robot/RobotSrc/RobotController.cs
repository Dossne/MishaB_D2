using System.Collections.Generic;
using VacuumSorter.PlayerInput;
using UnityEngine;

namespace VacuumSorter.Robot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RobotController : MonoBehaviour
    {
        private const float VisualY = 0.3f;

        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private RobotConfig _config;

        private readonly ContactPoint[] _contactBuffer = new ContactPoint[8];
        private readonly Collider[] _ejectOverlapBuffer = new Collider[48];
        private readonly HashSet<Rigidbody> _ejectRigidbodies = new();

        private Rigidbody _rigidbody;
        private bool _isInitialized;
        private bool _isTouchingSideObstacle;
        private Vector3 _lastObstacleNormal;

        public void Initialize(PlayerInputReader inputReader, RobotConfig config)
        {
            _inputReader = inputReader;
            _config = config;
            SetupPhysics();
            EnsureRobotVisuals();
            _isInitialized = _inputReader != null && _config != null;
        }

        public void EjectItemsFromScoop()
        {
            var center = transform.TransformPoint(new Vector3(0f, 0.22f, 0.84f));
            var halfExtents = new Vector3(0.34f, 0.24f, 0.52f);
            var overlapCount = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                _ejectOverlapBuffer,
                transform.rotation,
                ~0,
                QueryTriggerInteraction.Ignore);

            _ejectRigidbodies.Clear();

            for (var i = 0; i < overlapCount; i++)
            {
                var hit = _ejectOverlapBuffer[i];
                if (hit == null)
                {
                    continue;
                }

                var hitBody = hit.attachedRigidbody;
                if (hitBody == null || hitBody == _rigidbody)
                {
                    continue;
                }

                _ejectRigidbodies.Add(hitBody);
            }

            var ejectDirection = (transform.forward * 1.08f + Vector3.up * 0.22f).normalized;
            foreach (var body in _ejectRigidbodies)
            {
                body.AddForce(ejectDirection * 8.4f, ForceMode.VelocityChange);
            }
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

            var currentVelocity = _rigidbody.linearVelocity;
            var currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            var hasInput = inputMagnitude > 0.001f;
            var acceleration = hasInput ? _config.Acceleration : _config.Deceleration;
            var targetVelocity = Vector3.zero;
            var shouldRotateToVelocity = false;
            var desiredDirection = Vector3.zero;

            if (hasInput)
            {
                desiredDirection = new Vector3(rawInput.x, 0f, rawInput.y).normalized;
                var targetSpeed = shapedMagnitude * _config.MaxSpeed;
                targetVelocity = desiredDirection * targetSpeed;
                shouldRotateToVelocity = true;
            }

            var maxDelta = acceleration * Time.fixedDeltaTime;
            var nextPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, maxDelta);
            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, 0f, nextPlanarVelocity.z);

            var blockedByWallForTurn = _isTouchingSideObstacle
                && desiredDirection.sqrMagnitude > 0.001f
                && Vector3.Dot(desiredDirection, -_lastObstacleNormal) > 0.12f;

            if (!blockedByWallForTurn && shouldRotateToVelocity && nextPlanarVelocity.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(nextPlanarVelocity.normalized, Vector3.up);
                var rotationStep = _config.TurnSpeed * Time.fixedDeltaTime;
                _rigidbody.MoveRotation(Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, rotationStep));
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.rigidbody != null)
            {
                return;
            }

            var contactCount = collision.GetContacts(_contactBuffer);
            var normalSum = Vector3.zero;
            var sideContacts = 0;

            for (var i = 0; i < contactCount; i++)
            {
                var normal = _contactBuffer[i].normal;
                if (normal.y > 0.45f)
                {
                    continue;
                }

                normal.y = 0f;
                if (normal.sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                normalSum += normal.normalized;
                sideContacts++;
            }

            if (sideContacts > 0)
            {
                _isTouchingSideObstacle = true;
                _lastObstacleNormal = (normalSum / sideContacts).normalized;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.rigidbody != null)
            {
                return;
            }

            _isTouchingSideObstacle = false;
            _lastObstacleNormal = Vector3.zero;
        }

        private void SetupPhysics()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.mass = 5.5f;
            _rigidbody.linearDamping = 4.5f;
            _rigidbody.angularDamping = 20f;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.solverIterations = 12;
            _rigidbody.solverVelocityIterations = 12;
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var bodyCollider = GetComponent<CapsuleCollider>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            bodyCollider.radius = 0.56f;
            bodyCollider.height = 0.58f;
            bodyCollider.center = new Vector3(0f, VisualY, -0.1f);
            bodyCollider.direction = 1;
        }

        private void EnsureRobotVisuals()
        {
            var body = EnsureVisualCube("RobotBody", transform, new Vector3(0f, VisualY, -0.05f), new Vector3(1.3f, 0.46f, 1.45f), new Color(0.72f, 0.79f, 0.86f, 1f));
            var top = EnsureVisualCube("RobotTop", transform, new Vector3(0f, 0.5f, -0.08f), new Vector3(0.8f, 0.18f, 0.8f), new Color(0.18f, 0.22f, 0.27f, 1f));
            EnsureVisualCube("ScoopLeft", transform, new Vector3(-0.58f, 0.22f, 0.8f), new Vector3(0.22f, 0.3f, 1.35f), new Color(0.97f, 0.78f, 0.32f, 1f));
            EnsureVisualCube("ScoopRight", transform, new Vector3(0.58f, 0.22f, 0.8f), new Vector3(0.22f, 0.3f, 1.35f), new Color(0.97f, 0.78f, 0.32f, 1f));

            body.transform.SetSiblingIndex(0);
            top.transform.SetSiblingIndex(1);

            RemoveChildIfExists("ScoopBack");
            RemoveChildIfExists("ScoopLip");
            RemoveChildIfExists("ScoopColliderBack");
            RemoveChildIfExists("ScoopColliderLip");

            EnsureScoopCollider("ScoopColliderLeft", new Vector3(-0.58f, 0.22f, 0.8f), new Vector3(0.22f, 0.3f, 1.35f));
            EnsureScoopCollider("ScoopColliderRight", new Vector3(0.58f, 0.22f, 0.8f), new Vector3(0.22f, 0.3f, 1.35f));
        }

        private static GameObject EnsureVisualCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var existing = parent.Find(name);
            GameObject cube;

            if (existing == null)
            {
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = name;
                cube.transform.SetParent(parent, false);
            }
            else
            {
                cube = existing.gameObject;
            }

            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
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

        private void RemoveChildIfExists(string name)
        {
            var child = transform.Find(name);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
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