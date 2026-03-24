using System.Collections;
using System.Collections.Generic;
using VacuumSorter.Bootstrap;
using VacuumSorter.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacuumSorter.LevelFlow
{
    [DisallowMultipleComponent]
    public sealed class LevelBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage4LevelRuntime";

        [SerializeField] private Transform _itemsRoot;

        private readonly List<ItemTypeConfig> _spawnPlanBuffer = new();

        private bool _isInitialized;
        private Coroutine _spawnRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
                DontDestroyOnLoad(host);
            }

            var bootstrap = host.GetComponent<LevelBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<LevelBootstrap>();
            }

            bootstrap.InitializeIfReady();
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
            InitializeIfReady();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopSpawnRoutine();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetForSceneLoad();
            InitializeIfReady();
        }

        private void ResetForSceneLoad()
        {
            _isInitialized = false;
            StopSpawnRoutine();
            _spawnPlanBuffer.Clear();

            if (_itemsRoot != null)
            {
                Destroy(_itemsRoot.gameObject);
                _itemsRoot = null;
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
                Debug.LogError("Stage7 bootstrap: cannot resolve active LevelConfig for current level index.", levelCatalog);
                return;
            }

            var totalItems = BuildSpawnPlan(activeLevelConfig, _spawnPlanBuffer);
            if (totalItems <= 0)
            {
                Debug.LogError("Stage7 bootstrap: active LevelConfig has no valid item batches.", activeLevelConfig);
                return;
            }

            EnsureItemsRoot();
            StopSpawnRoutine();
            _spawnRoutine = StartCoroutine(SpawnItems(activeLevelConfig.Spawn, _spawnPlanBuffer));

            _isInitialized = true;
            Debug.Log("Stage7 bootstrap: content-driven level item spawn initialized.");
        }

        private static int BuildSpawnPlan(LevelConfig levelConfig, List<ItemTypeConfig> output)
        {
            output.Clear();
            if (levelConfig == null || levelConfig.ItemBatches == null)
            {
                return 0;
            }

            for (var i = 0; i < levelConfig.ItemBatches.Count; i++)
            {
                var batch = levelConfig.ItemBatches[i];
                if (batch == null || batch.ItemType == null || batch.Count <= 0)
                {
                    continue;
                }

                for (var c = 0; c < batch.Count; c++)
                {
                    output.Add(batch.ItemType);
                }
            }

            return output.Count;
        }

        private void EnsureItemsRoot()
        {
            if (_itemsRoot != null)
            {
                return;
            }

            var existing = transform.Find("ItemsRuntimeRoot");
            if (existing != null)
            {
                _itemsRoot = existing;
                return;
            }

            var itemsRootObject = new GameObject("ItemsRuntimeRoot");
            _itemsRoot = itemsRootObject.transform;
            _itemsRoot.SetParent(transform, false);
            _itemsRoot.localPosition = Vector3.zero;
            _itemsRoot.localRotation = Quaternion.identity;
            _itemsRoot.localScale = Vector3.one;
        }

        private IEnumerator SpawnItems(LevelConfig.SpawnSettings spawnSettings, IReadOnlyList<ItemTypeConfig> spawnPlan)
        {
            if (spawnSettings == null || spawnPlan == null)
            {
                _spawnRoutine = null;
                yield break;
            }

            for (var i = 0; i < spawnPlan.Count; i++)
            {
                var itemType = spawnPlan[i];
                if (itemType == null)
                {
                    continue;
                }

                var position = BuildSpawnPosition(spawnSettings, i);
                SpawnSingleItem(itemType, position);

                if (spawnSettings.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnSettings.SpawnInterval);
                }
            }

            _spawnRoutine = null;
        }

        private static Vector3 BuildSpawnPosition(LevelConfig.SpawnSettings spawnSettings, int index)
        {
            var random2D = Random.insideUnitCircle * spawnSettings.SpawnRadius;
            var layer = index / 8;
            var y = spawnSettings.SpawnHeight + layer * spawnSettings.HeightStep;
            return new Vector3(random2D.x, y, random2D.y);
        }

        private void SpawnSingleItem(ItemTypeConfig itemType, Vector3 position)
        {
            var itemObject = GameObject.CreatePrimitive(itemType.PrimitiveType);
            itemObject.transform.SetParent(_itemsRoot, true);
            itemObject.transform.position = position;
            itemObject.transform.rotation = Random.rotation;

            var rigidbody = itemObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = true;

            var itemView = itemObject.AddComponent<ItemView>();
            itemView.Initialize(itemType);
        }

        private void StopSpawnRoutine()
        {
            if (_spawnRoutine == null)
            {
                return;
            }

            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }
}
