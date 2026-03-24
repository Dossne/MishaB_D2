using VacuumSorter.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacuumSorter.Progression
{
    [DisallowMultipleComponent]
    public sealed class ProgressionRuntimeBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage9ProgressionRuntime";

        [SerializeField] private UpgradeConfig _upgradeConfig;

        private bool _isInitialized;

        public static UpgradeService UpgradeService { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetOnAppStart()
        {
            UpgradeService = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
                DontDestroyOnLoad(host);
            }

            var bootstrap = host.GetComponent<ProgressionRuntimeBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<ProgressionRuntimeBootstrap>();
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
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isInitialized = false;
            InitializeIfReady();
        }

        private void InitializeIfReady()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_upgradeConfig == null)
            {
                var services = ServiceLocator.Current;
                if (services == null || services.ConfigurationProvider == null)
                {
                    return;
                }

                if (!services.ConfigurationProvider.TryGetConfig(out _upgradeConfig) || _upgradeConfig == null)
                {
                    Debug.LogError("Stage9 progression: UpgradeConfig is not assigned in ConfigurationProvider.", services.ConfigurationProvider);
                    return;
                }
            }

            if (UpgradeService == null)
            {
                UpgradeService = new UpgradeService(_upgradeConfig);
                Debug.Log("Stage9 progression: upgrade service initialized.");
            }

            _isInitialized = true;
        }
    }
}
