using System.Collections.Generic;
using VacuumSorter.Bootstrap;
using VacuumSorter.Items;
using VacuumSorter.MainUI;
using VacuumSorter.Scoring;
using UnityEngine;

namespace VacuumSorter.SortTargets
{
    [DisallowMultipleComponent]
    public sealed class SortTargetSliceBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage5SortTargetsRuntime";

        [SerializeField] private Transform _targetsRoot;

        private bool _isInitialized;
        private ScoreService _scoreService;
        private MainUiProvider _mainUiProvider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var services = ServiceLocator.Current;
            GameObject host = services != null ? services.gameObject : null;

            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
            }

            var bootstrap = host.GetComponent<SortTargetSliceBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<SortTargetSliceBootstrap>();
            }

            bootstrap.InitializeIfReady();
        }

        private void Start()
        {
            InitializeIfReady();
        }

        private void InitializeIfReady()
        {
            if (_isInitialized)
            {
                return;
            }

            var services = ServiceLocator.Current;
            if (services == null || services.ConfigurationProvider == null)
            {
                return;
            }

            SortTargetConfig sortTargetConfig;
            if (!services.ConfigurationProvider.TryGetConfig(out sortTargetConfig) || sortTargetConfig == null)
            {
                Debug.LogError("Stage5 bootstrap: SortTargetConfig is not assigned in ConfigurationProvider.", services.ConfigurationProvider);
                return;
            }

            var validTargets = CollectValidTargets(sortTargetConfig);
            if (validTargets.Count < 2)
            {
                Debug.LogError("Stage5 bootstrap: configure at least 2 valid sort targets with item types.", sortTargetConfig);
                return;
            }

            _mainUiProvider = services.MainUiProvider;
            _scoreService = new ScoreService(validTargets);

            EnsureTargetsRoot();
            SpawnTargets(validTargets, sortTargetConfig);
            UpdateHud();

            _isInitialized = true;
            Debug.Log("Stage5 bootstrap: sorting targets initialized.");
        }

        private List<SortTargetConfig.TargetDefinition> CollectValidTargets(SortTargetConfig config)
        {
            var result = new List<SortTargetConfig.TargetDefinition>();
            if (config.Targets == null)
            {
                return result;
            }

            for (var i = 0; i < config.Targets.Count; i++)
            {
                var target = config.Targets[i];
                if (target == null || target.ItemType == null || target.RequiredCount <= 0)
                {
                    continue;
                }

                result.Add(target);
            }

            return result;
        }

        private void EnsureTargetsRoot()
        {
            if (_targetsRoot != null)
            {
                return;
            }

            var existing = transform.Find("SortTargetsRuntimeRoot");
            if (existing != null)
            {
                _targetsRoot = existing;
                return;
            }

            var root = new GameObject("SortTargetsRuntimeRoot");
            _targetsRoot = root.transform;
            _targetsRoot.SetParent(transform, false);
            _targetsRoot.localPosition = Vector3.zero;
            _targetsRoot.localRotation = Quaternion.identity;
            _targetsRoot.localScale = Vector3.one;
        }

        private void SpawnTargets(
            IReadOnlyList<SortTargetConfig.TargetDefinition> definitions,
            SortTargetConfig config)
        {
            var anchorPoints = ResolveAnchorPoints();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                var anchorPosition = ResolveAnchorPosition(anchorPoints, definition.AnchorIndex, i);

                var targetObject = new GameObject(definition.ItemType.TypeId + "_Target");
                targetObject.transform.SetParent(_targetsRoot, false);
                targetObject.transform.position = anchorPosition;
                targetObject.transform.rotation = Quaternion.identity;

                var targetView = targetObject.AddComponent<SortTargetView>();
                targetView.Initialize(definition.ItemType, config, OnItemAccepted);
            }
        }

        private static List<Transform> ResolveAnchorPoints()
        {
            var result = new List<Transform>();
            var anchorRoot = GameObject.Find("SortTargetAnchors");
            if (anchorRoot == null)
            {
                return result;
            }

            var rootTransform = anchorRoot.transform;
            for (var i = 0; i < rootTransform.childCount; i++)
            {
                var child = rootTransform.GetChild(i);
                if (!child.name.StartsWith("SortAnchor_", System.StringComparison.Ordinal) || child.name.EndsWith("_Marker", System.StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(child);
            }

            result.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return result;
        }

        private static Vector3 ResolveAnchorPosition(List<Transform> anchors, int anchorIndex, int fallbackIndex)
        {
            if (anchors != null && anchors.Count > 0)
            {
                var safeIndex = Mathf.Clamp(anchorIndex, 0, anchors.Count - 1);
                return anchors[safeIndex].position;
            }

            var angle = fallbackIndex * Mathf.PI * 2f / 3f;
            return new Vector3(Mathf.Cos(angle) * 5.2f, 0f, Mathf.Sin(angle) * 5.2f);
        }

        private void OnItemAccepted(ItemTypeConfig acceptedType)
        {
            if (_scoreService == null || !_scoreService.TryRegisterSorted(acceptedType))
            {
                return;
            }

            UpdateHud();
        }

        private void UpdateHud()
        {
            if (_mainUiProvider == null || _scoreService == null)
            {
                return;
            }

            if (_mainUiProvider.ScoreLabel != null)
            {
                _mainUiProvider.ScoreLabel.text = $"Sorted: {_scoreService.TotalSorted}/{_scoreService.TotalRequired}";
            }

            if (_mainUiProvider.LevelLabel != null)
            {
                _mainUiProvider.LevelLabel.text = $"Score: {_scoreService.Score}";
            }

            if (_mainUiProvider.StateLabel != null)
            {
                _mainUiProvider.StateLabel.text = _scoreService.IsComplete
                    ? "State: Sorting target reached"
                    : "State: Push items into matching holes";
            }
        }
    }
}