using VacuumSorter.Bootstrap;
using VacuumSorter.MainUI;
using VacuumSorter.Meta;
using VacuumSorter.Progression;
using VacuumSorter.SortTargets;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VacuumSorter.LevelFlow
{
    [DisallowMultipleComponent]
    public sealed class LevelStateController : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage6LevelStateRuntime";

        [SerializeField] private RestartButtonPresenter _restartButtonPresenter;

        private MainUiProvider _mainUiProvider;
        private SortTargetSliceBootstrap _sortTargetSliceBootstrap;
        private LevelCompletionService _completionService;
        private bool _isInitialized;
        private bool _isCompletionBound;
        private bool _isLevelComplete;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
                DontDestroyOnLoad(host);
            }

            var controller = host.GetComponent<LevelStateController>();
            if (controller == null)
            {
                controller = host.AddComponent<LevelStateController>();
            }

            controller.InitializeIfReady();
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

            if (_sortTargetSliceBootstrap != null
                && _sortTargetSliceBootstrap.IsInitialized
                && _sortTargetSliceBootstrap.CompletionService != _completionService)
            {
                BindCompletionService(_sortTargetSliceBootstrap.CompletionService);
                _isInitialized = _completionService != null;
            }

            if (!_isLevelComplete)
            {
                return;
            }

            if (IsRestartPressed())
            {
                RestartCurrentLevel();
                return;
            }

            if (IsNextPressed())
            {
                GoToNextLevel();
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnbindSortTargetBootstrap();
            UnbindCompletionService();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetForSceneLoad();
            InitializeIfReady();
        }

        private void ResetForSceneLoad()
        {
            _isInitialized = false;
            _isLevelComplete = false;

            UnbindSortTargetBootstrap();
            UnbindCompletionService();

            _mainUiProvider = null;
            _sortTargetSliceBootstrap = null;
            _completionService = null;

            if (_restartButtonPresenter != null)
            {
                _restartButtonPresenter.Hide();
            }
        }

        private void InitializeIfReady()
        {
            if (_isInitialized)
            {
                return;
            }

            var services = ServiceLocator.Current;
            if (services == null || services.MainUiProvider == null)
            {
                return;
            }

            _mainUiProvider = services.MainUiProvider;

            EnsureRestartPresenter();

            _sortTargetSliceBootstrap = SortTargetSliceBootstrap.Current;
            if (_sortTargetSliceBootstrap == null)
            {
                return;
            }

            _sortTargetSliceBootstrap.Initialized -= OnSortTargetsInitialized;
            _sortTargetSliceBootstrap.Initialized += OnSortTargetsInitialized;

            if (_sortTargetSliceBootstrap.IsInitialized)
            {
                BindCompletionService(_sortTargetSliceBootstrap.CompletionService);
            }

            _isInitialized = _completionService != null;
        }

        private void EnsureRestartPresenter()
        {
            if (_restartButtonPresenter == null)
            {
                _restartButtonPresenter = GetComponent<RestartButtonPresenter>();
            }

            if (_restartButtonPresenter == null)
            {
                _restartButtonPresenter = gameObject.AddComponent<RestartButtonPresenter>();
            }

            _restartButtonPresenter.Initialize(_mainUiProvider);
            _restartButtonPresenter.Hide();
        }

        private void OnSortTargetsInitialized()
        {
            if (_sortTargetSliceBootstrap == null)
            {
                return;
            }

            BindCompletionService(_sortTargetSliceBootstrap.CompletionService);
            _isInitialized = _completionService != null;
        }

        private void BindCompletionService(LevelCompletionService completionService)
        {
            if (completionService == null)
            {
                return;
            }

            if (_completionService == completionService && _isCompletionBound)
            {
                return;
            }

            UnbindCompletionService();

            _completionService = completionService;
            _completionService.SortedRegistered += OnSortedRegistered;
            _completionService.ProgressChanged += OnProgressChanged;
            _completionService.Completed += OnCompleted;
            _isCompletionBound = true;
            _isLevelComplete = _completionService.IsCompleted;

            RefreshHud();
        }

        private void UnbindCompletionService()
        {
            if (!_isCompletionBound || _completionService == null)
            {
                return;
            }

            _completionService.SortedRegistered -= OnSortedRegistered;
            _completionService.ProgressChanged -= OnProgressChanged;
            _completionService.Completed -= OnCompleted;
            _isCompletionBound = false;
        }

        private void UnbindSortTargetBootstrap()
        {
            if (_sortTargetSliceBootstrap == null)
            {
                return;
            }

            _sortTargetSliceBootstrap.Initialized -= OnSortTargetsInitialized;
        }

        private void OnSortedRegistered(int scoreDelta)
        {
            var upgradeService = ProgressionRuntimeBootstrap.UpgradeService;
            if (upgradeService == null || scoreDelta <= 0)
            {
                return;
            }

            upgradeService.AddCollectedScore(scoreDelta);
        }

        private void OnProgressChanged()
        {
            RefreshHud();
        }

        private void OnCompleted()
        {
            _isLevelComplete = true;
            RefreshHud();

            if (_restartButtonPresenter != null)
            {
                var upgradeService = ProgressionRuntimeBootstrap.UpgradeService;
                var scoopLevel = upgradeService != null ? upgradeService.CurrentScoopLevel : 0;
                var scoopMultiplier = upgradeService != null ? upgradeService.CurrentScoopMultiplier : 1f;
                _restartButtonPresenter.ShowCompletion(
                    LevelRuntimeState.CurrentLevelNumber,
                    scoopLevel,
                    scoopMultiplier,
                    RestartCurrentLevel,
                    GoToNextLevel);
            }
        }

        private void RefreshHud()
        {
            if (_mainUiProvider == null || _completionService == null)
            {
                return;
            }

            if (_mainUiProvider.ScoreLabel != null)
            {
                _mainUiProvider.ScoreLabel.text = $"Score: {_completionService.Score}";
            }

            if (_mainUiProvider.LevelLabel != null)
            {
                _mainUiProvider.LevelLabel.text = $"Level: {LevelRuntimeState.CurrentLevelNumber}";
            }

            if (_mainUiProvider.StateLabel != null)
            {
                _mainUiProvider.StateLabel.text = _completionService.IsCompleted
                    ? "Remaining: 0 (Complete)"
                    : $"Remaining: {_completionService.RemainingRequired}";
            }

            var upgradeService = ProgressionRuntimeBootstrap.UpgradeService;
            if (_mainUiProvider.UpgradeLabel != null && upgradeService != null)
            {
                _mainUiProvider.UpgradeLabel.text = $"Scoop: Lv{upgradeService.CurrentScoopLevel} x{upgradeService.CurrentScoopMultiplier:0.00} (Total {upgradeService.TotalCollectedScore})";
            }
            else if (_mainUiProvider.UpgradeLabel != null)
            {
                _mainUiProvider.UpgradeLabel.text = "Scoop: Lv0 x1.00 (Total 0)";
            }
        }

        private static bool IsRestartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }

        private static bool IsNextPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.nKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.N);
#endif
        }

        private static void RestartCurrentLevel()
        {
            LevelRuntimeState.RestartCurrentLevel();
            ReloadActiveScene();
        }

        private static void GoToNextLevel()
        {
            var services = ServiceLocator.Current;
            var levelCount = 1;
            if (services != null && services.ConfigurationProvider != null)
            {
                LevelCatalogConfig levelCatalog;
                if (services.ConfigurationProvider.TryGetConfig(out levelCatalog) && levelCatalog != null)
                {
                    levelCount = Mathf.Max(1, levelCatalog.LevelCount);
                }
            }

            LevelRuntimeState.AdvanceToNextLevel(levelCount);
            ReloadActiveScene();
        }

        private static void ReloadActiveScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
            {
                SceneManager.LoadScene(activeScene.path);
                return;
            }

            SceneManager.LoadScene(activeScene.name);
        }
    }
}
