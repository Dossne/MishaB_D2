using UnityEngine;

namespace VacuumSorter.GameCamera
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GameCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Vector3 _lookAtWorldPosition = Vector3.zero;

        [Header("Orientation")]
        [SerializeField, Range(60f, 90f)] private float _pitchDegrees = 85f;
        [SerializeField] private float _yawDegrees;
        [SerializeField, Min(5f)] private float _cameraDistance = 20f;

        [Header("Arena framing")]
        [SerializeField, Min(1f)] private float _arenaHalfWidth = 8f;
        [SerializeField, Min(1f)] private float _arenaHalfDepth = 8f;
        [SerializeField, Min(0f)] private float _padding = 1.5f;
        [SerializeField, Min(1f)] private float _minimumOrthoSize = 8.5f;
        [SerializeField, Min(0.2f)] private float _minimumAspect = 9f / 16f;

        private Camera _camera;
        private float _lastAspect = -1f;

        private void OnEnable()
        {
            EnsureCamera();
            ApplyFraming();
        }

        private void OnValidate()
        {
            EnsureCamera();
            ApplyFraming();
        }

        private void LateUpdate()
        {
            EnsureCamera();
            if (Mathf.Abs(_lastAspect - _camera.aspect) > 0.0001f)
            {
                ApplyFraming();
            }
        }

        public void ConfigureArena(float arenaHalfWidth, float arenaHalfDepth, Vector3 lookAtWorldPosition)
        {
            _arenaHalfWidth = Mathf.Max(1f, arenaHalfWidth);
            _arenaHalfDepth = Mathf.Max(1f, arenaHalfDepth);
            _lookAtWorldPosition = lookAtWorldPosition;
            ApplyFraming();
        }

        private void EnsureCamera()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            _camera.orthographic = true;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 100f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
        }

        private void ApplyFraming()
        {
            if (_camera == null)
            {
                return;
            }

            var clampedAspect = Mathf.Max(_camera.aspect, _minimumAspect);
            var requiredHalfHeight = Mathf.Max(
                _arenaHalfDepth + _padding,
                (_arenaHalfWidth + _padding) / clampedAspect);

            _camera.orthographicSize = Mathf.Max(_minimumOrthoSize, requiredHalfHeight);

            var rotation = Quaternion.Euler(_pitchDegrees, _yawDegrees, 0f);
            transform.SetPositionAndRotation(_lookAtWorldPosition - rotation * Vector3.forward * _cameraDistance, rotation);

            _lastAspect = _camera.aspect;
        }
    }
}