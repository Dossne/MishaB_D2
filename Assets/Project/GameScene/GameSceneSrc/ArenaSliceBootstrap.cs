using VacuumSorter.Bootstrap;
using VacuumSorter.GameCamera;
using UnityEngine;

namespace VacuumSorter.GameScene
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ArenaSliceBootstrap : MonoBehaviour
    {
        private const string ArenaRootName = "ArenaRoot";
        private const string ArenaFloorName = "ArenaFloor";
        private const string ArenaBoundsName = "ArenaBounds";
        private const string ArenaWallsName = "ArenaWalls";
        private const string CenterSpawnZoneName = "CenterSpawnZone";
        private const string SortAnchorRootName = "SortTargetAnchors";

        [Header("Arena size")]
        [SerializeField, Min(8f)] private float _arenaWidth = 16f;
        [SerializeField, Min(8f)] private float _arenaDepth = 16f;

        [Header("Arena bounds")]
        [SerializeField, Min(0.1f)] private float _floorThickness = 0.4f;
        [SerializeField, Min(0.2f)] private float _wallHeight = 1.6f;
        [SerializeField, Min(0.2f)] private float _wallThickness = 0.6f;

        [Header("Markers")]
        [SerializeField, Min(1f)] private float _centerZoneDiameter = 4f;
        [SerializeField, Min(2f)] private float _anchorRadius = 5.2f;
        [SerializeField, Min(1)] private int _anchorCount = 6;

        [Header("Colors")]
        [SerializeField] private Color _floorColor = new Color(0.23f, 0.28f, 0.33f, 1f);
        [SerializeField] private Color _wallColor = new Color(0.13f, 0.16f, 0.2f, 1f);
        [SerializeField] private Color _centerColor = new Color(0.88f, 0.74f, 0.24f, 1f);
        [SerializeField] private Color _anchorColor = new Color(0.2f, 0.67f, 0.69f, 1f);

        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static PhysicsMaterial s_lowFrictionFloorMaterial;

        private MaterialPropertyBlock _propertyBlock;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeArena()
        {
            var services = ServiceLocator.Current;
            var host = services != null ? services.gameObject : null;

            if (host == null)
            {
                host = new GameObject("ArenaSliceRuntimeBootstrap");
            }

            var existing = host.GetComponent<ArenaSliceBootstrap>();
            if (existing == null)
            {
                existing = host.AddComponent<ArenaSliceBootstrap>();
            }

            existing.RebuildArena();
        }

        private void OnEnable()
        {
            RebuildArena();
        }

        private void OnValidate()
        {
            RebuildArena();
        }

        public void RebuildArena()
        {
            var arenaRoot = FindOrCreateChild(transform, ArenaRootName);
            EnsureArena(arenaRoot);
            EnsureFixedCamera();
        }

        private void EnsureArena(Transform arenaRoot)
        {
            arenaRoot.localPosition = Vector3.zero;
            arenaRoot.localRotation = Quaternion.identity;
            arenaRoot.localScale = Vector3.one;

            var floor = EnsurePrimitive(
                arenaRoot,
                ArenaFloorName,
                PrimitiveType.Cube,
                new Vector3(0f, -_floorThickness * 0.5f, 0f),
                new Vector3(_arenaWidth, _floorThickness, _arenaDepth));
            SetColor(floor, _floorColor);
            ApplyLowFrictionFloorMaterial(floor);

            var bounds = EnsureChild(arenaRoot, ArenaBoundsName, Vector3.zero, Vector3.one);
            var walls = EnsureChild(bounds, ArenaWallsName, Vector3.zero, Vector3.one);
            EnsureWall(walls, "WallTop", new Vector3(0f, _wallHeight * 0.5f, _arenaDepth * 0.5f), new Vector3(_arenaWidth + _wallThickness * 2f, _wallHeight, _wallThickness));
            EnsureWall(walls, "WallBottom", new Vector3(0f, _wallHeight * 0.5f, -_arenaDepth * 0.5f), new Vector3(_arenaWidth + _wallThickness * 2f, _wallHeight, _wallThickness));
            EnsureWall(walls, "WallLeft", new Vector3(-_arenaWidth * 0.5f, _wallHeight * 0.5f, 0f), new Vector3(_wallThickness, _wallHeight, _arenaDepth));
            EnsureWall(walls, "WallRight", new Vector3(_arenaWidth * 0.5f, _wallHeight * 0.5f, 0f), new Vector3(_wallThickness, _wallHeight, _arenaDepth));

            var centerScale = new Vector3(_centerZoneDiameter, 0.02f, _centerZoneDiameter);
            var centerZone = EnsurePrimitive(arenaRoot, CenterSpawnZoneName, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), centerScale);
            SetColor(centerZone, _centerColor);
            MakeMarkerNonBlocking(centerZone);

            var sortAnchorRoot = EnsureChild(arenaRoot, SortAnchorRootName, Vector3.zero, Vector3.one);
            for (var i = 0; i < _anchorCount; i++)
            {
                var angleRad = Mathf.Deg2Rad * (i * 360f / _anchorCount);
                var position = new Vector3(Mathf.Cos(angleRad) * _anchorRadius, 0.01f, Mathf.Sin(angleRad) * _anchorRadius);
                var anchor = EnsureChild(sortAnchorRoot, string.Format("SortAnchor_{0:00}", i + 1), position, Vector3.one);
                EnsureAnchorMarker(sortAnchorRoot, anchor.name, position);
            }

            CleanupExtraAnchors(sortAnchorRoot);
        }

        private void EnsureWall(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var wall = EnsurePrimitive(parent, name, PrimitiveType.Cube, localPosition, localScale);
            SetColor(wall, _wallColor);
        }

        private void EnsureAnchorMarker(Transform parent, string anchorName, Vector3 localPosition)
        {
            var marker = EnsurePrimitive(parent, anchorName + "_Marker", PrimitiveType.Cylinder, localPosition, new Vector3(0.9f, 0.02f, 0.9f));
            SetColor(marker, _anchorColor);
            MakeMarkerNonBlocking(marker);
        }

        private static void MakeMarkerNonBlocking(GameObject marker)
        {
            var collider = marker.GetComponent<Collider>();
            if (collider == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        private void CleanupExtraAnchors(Transform sortAnchorRoot)
        {
            for (var i = sortAnchorRoot.childCount - 1; i >= 0; i--)
            {
                var child = sortAnchorRoot.GetChild(i);
                if (!child.name.StartsWith("SortAnchor_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                var suffix = child.name.Substring("SortAnchor_".Length);
                if (!int.TryParse(suffix, out var index) || index <= _anchorCount)
                {
                    continue;
                }

                DestroyImmediate(child.gameObject);

                var marker = sortAnchorRoot.Find(string.Format("SortAnchor_{0:00}_Marker", index));
                if (marker != null)
                {
                    DestroyImmediate(marker.gameObject);
                }
            }
        }

        private void EnsureFixedCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var controller = mainCamera.GetComponent<GameCameraController>();
            if (controller == null)
            {
                controller = mainCamera.gameObject.AddComponent<GameCameraController>();
            }

            controller.ConfigureArena(_arenaWidth * 0.5f, _arenaDepth * 0.5f, Vector3.zero);
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            return child;
        }

        private static GameObject EnsurePrimitive(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = Quaternion.identity;
                existing.localScale = localScale;
                return existing.gameObject;
            }

            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            return primitive;
        }

        private static Transform EnsureChild(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = Quaternion.identity;
                existing.localScale = localScale;
                return existing;
            }

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = localScale;
            return child;
        }

        private void SetColor(GameObject gameObject, Color color)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorPropertyId, color);
            _propertyBlock.SetColor(ColorPropertyId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private static void ApplyLowFrictionFloorMaterial(GameObject floorObject)
        {
            var floorCollider = floorObject.GetComponent<Collider>();
            if (floorCollider == null)
            {
                return;
            }

            if (s_lowFrictionFloorMaterial == null)
            {
                s_lowFrictionFloorMaterial = new PhysicsMaterial("FloorLowFriction")
                {
                    dynamicFriction = 0.02f,
                    staticFriction = 0.02f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounciness = 0f,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
            }

            floorCollider.sharedMaterial = s_lowFrictionFloorMaterial;
        }
    }
}