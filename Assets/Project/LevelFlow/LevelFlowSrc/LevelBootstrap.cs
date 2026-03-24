using System.Collections;
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

            LevelSpawnConfig levelSpawnConfig;
            if (!services.ConfigurationProvider.TryGetConfig(out levelSpawnConfig) || levelSpawnConfig == null)
            {
                Debug.LogError("Stage4 bootstrap: LevelSpawnConfig is not assigned in ConfigurationProvider.", services.ConfigurationProvider);
                return;
            }

            if (levelSpawnConfig.ItemTypes == null || levelSpawnConfig.ItemTypes.Count < 2)
            {
                Debug.LogError("Stage4 bootstrap: configure at least 2 item types in LevelSpawnConfig.", levelSpawnConfig);
                return;
            }

            EnsureItemsRoot();
            StopSpawnRoutine();
            _spawnRoutine = StartCoroutine(SpawnItems(levelSpawnConfig));

            _isInitialized = true;
            Debug.Log("Stage4 bootstrap: falling pile spawn initialized.");
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

        private IEnumerator SpawnItems(LevelSpawnConfig config)
        {
            var itemTypes = config.ItemTypes;
            for (var i = 0; i < config.TotalItemCount; i++)
            {
                var itemType = itemTypes[i % itemTypes.Count];
                if (itemType == null)
                {
                    continue;
                }

                var position = BuildSpawnPosition(config, i);
                SpawnSingleItem(itemType, position);

                if (config.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(config.SpawnInterval);
                }
            }

            _spawnRoutine = null;
        }

        private Vector3 BuildSpawnPosition(LevelSpawnConfig config, int index)
        {
            var random2D = Random.insideUnitCircle * config.SpawnRadius;
            var layer = index / 8;
            var y = config.SpawnHeight + layer * config.HeightStep;
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
