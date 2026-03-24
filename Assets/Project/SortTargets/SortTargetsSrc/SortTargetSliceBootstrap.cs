using System;
using System.Collections.Generic;
using VacuumSorter.Bootstrap;
using VacuumSorter.Feedback;
using VacuumSorter.Items;
using VacuumSorter.LevelFlow;
using VacuumSorter.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacuumSorter.SortTargets
{
    [DisallowMultipleComponent]
    public sealed class SortTargetSliceBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage5SortTargetsRuntime";

        [SerializeField] private Transform _targetsRoot;

        private bool _isInitialized;
        private ScoreService _scoreService;
        private LevelCompletionService _completionService;

        public static SortTargetSliceBootstrap Current { get; private set; }

        public bool IsInitialized => _isInitialized;
        public LevelCompletionService CompletionService => _completionService;

        public event Action Initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
            }

            DontDestroyOnLoad(host);

            var bootstrap = host.GetComponent<SortTargetSliceBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<SortTargetSliceBootstrap>();
            }

            bootstrap.InitializeIfReady();
        }

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            InitializeIfReady();
        }

        private void Update()
        {
            if (_isInitialized && (_targetsRoot == null || _targetsRoot.childCount == 0))
            {
                ResetForSceneLoad();
            }

            InitializeIfReady();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetForSceneLoad();
            InitializeIfReady();
        }

        private void ResetForSceneLoad()
        {
            _isInitialized = false;
            _scoreService = null;
            _completionService = null;

            if (_targetsRoot != null)
            {
                var rootToDestroy = _targetsRoot.gameObject;
                _targetsRoot = null;
                Destroy(rootToDestroy);
            }
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

            LevelCatalogConfig levelCatalog;
            if (!services.ConfigurationProvider.TryGetConfig(out levelCatalog) || levelCatalog == null)
            {
                Debug.LogError("Stage7 bootstrap: LevelCatalogConfig is not assigned in ConfigurationProvider.", services.ConfigurationProvider);
                return;
            }

            LevelConfig activeLevelConfig;
            if (!levelCatalog.TryGetLevelByIndex(LevelRuntimeState.CurrentLevelIndex, out activeLevelConfig) || activeLevelConfig == null)
            {
                Debug.LogError("Stage7 bootstrap: cannot resolve active LevelConfig for sort targets.", levelCatalog);
                return;
            }

            var validTargets = CollectValidTargets(activeLevelConfig);
            if (validTargets.Count < 2)
            {
                Debug.LogError("Stage7 bootstrap: configure at least 2 valid targets with item types in LevelConfig.", activeLevelConfig);
                return;
            }

            _scoreService = new ScoreService(validTargets);
            _completionService = new LevelCompletionService(_scoreService);

            EnsureTargetsRoot();
            SpawnTargets(validTargets, activeLevelConfig.TargetInteraction);

            _isInitialized = true;
            Initialized?.Invoke();

            Debug.Log("Stage7 bootstrap: content-driven sorting targets initialized.");
        }

        private List<LevelConfig.TargetDefinition> CollectValidTargets(LevelConfig levelConfig)
        {
            var result = new List<LevelConfig.TargetDefinition>();
            if (levelConfig == null || levelConfig.Targets == null)
            {
                return result;
            }

            for (var i = 0; i < levelConfig.Targets.Count; i++)
            {
                var target = levelConfig.Targets[i];
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
            IReadOnlyList<LevelConfig.TargetDefinition> definitions,
            LevelConfig.TargetInteractionSettings interactionSettings)
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
                targetView.Initialize(definition.ItemType, interactionSettings, OnItemAccepted);
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

        private bool OnItemAccepted(ItemTypeConfig acceptedType, Vector3 acceptedWorldPosition)
        {
            var counted = _completionService != null && _completionService.TryRegisterSorted(acceptedType);
            if (counted)
            {
                FeedbackRuntimeBootstrap.Current?.PlaySortSuccess(acceptedType, acceptedWorldPosition);
            }

            return counted;
        }
    }
}
