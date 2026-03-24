using TMPro;
using VacuumSorter.Bootstrap;
using VacuumSorter.MainUI;
using VacuumSorter.PlayerInput;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace VacuumSorter.Robot
{
    [DisallowMultipleComponent]
    public sealed class Stage3PlayerMovementBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage3Runtime";

        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private JoystickView _joystickView;
        [SerializeField] private RobotController _robotController;
        [SerializeField] private Button _scoopEjectButton;

        private bool _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
                DontDestroyOnLoad(host);
            }

            var bootstrap = host.GetComponent<Stage3PlayerMovementBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<Stage3PlayerMovementBootstrap>();
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
            ResetForSceneLoad();
            InitializeIfReady();
        }

        private void ResetForSceneLoad()
        {
            _isInitialized = false;
            _eventSystem = null;
            _inputReader = null;
            _joystickView = null;
            _robotController = null;
            _scoopEjectButton = null;
        }

        private void InitializeIfReady()
        {
            if (_isInitialized)
            {
                return;
            }

            var services = ServiceLocator.Current;
            if (services == null)
            {
                return;
            }

            var mainUiProvider = services.MainUiProvider;
            if (mainUiProvider == null || mainUiProvider.HudParent == null)
            {
                return;
            }

            var configurationProvider = services.ConfigurationProvider;
            if (configurationProvider == null)
            {
                Debug.LogError("Stage3 bootstrap: ConfigurationProvider is missing in ServiceLocator.", services);
                return;
            }

            RobotConfig robotConfig;
            if (!configurationProvider.TryGetConfig(out robotConfig) || robotConfig == null)
            {
                Debug.LogError("Stage3 bootstrap: RobotConfig is not assigned in ConfigurationProvider.", configurationProvider);
                return;
            }

            EnsureEventSystem();
            EnsureInput(mainUiProvider);
            EnsureRobot(robotConfig);

            if (_inputReader == null || _robotController == null)
            {
                Debug.LogError("Stage3 bootstrap: required movement references are not ready.", this);
                return;
            }

            _robotController.Initialize(_inputReader, robotConfig);
            EnsureScoopEjectButton(mainUiProvider);

            _isInitialized = true;

            Debug.Log("Stage3 bootstrap: player input and robot movement initialized.");
        }

        private void EnsureEventSystem()
        {
            if (_eventSystem != null)
            {
                return;
            }

            _eventSystem = EventSystem.current;
            if (_eventSystem != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            _eventSystem = eventSystemObject.GetComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private void EnsureInput(MainUiProvider mainUiProvider)
        {
            if (_inputReader != null)
            {
                if (_joystickView != null)
                {
                    _inputReader.BindJoystick(_joystickView);
                }

                return;
            }

            var inputRoot = new GameObject("PlayerInputRoot", typeof(RectTransform), typeof(PlayerInputReader));
            var inputRootRect = inputRoot.GetComponent<RectTransform>();
            inputRootRect.SetParent(mainUiProvider.HudParent, false);
            inputRootRect.anchorMin = Vector2.zero;
            inputRootRect.anchorMax = Vector2.one;
            inputRootRect.offsetMin = Vector2.zero;
            inputRootRect.offsetMax = Vector2.zero;

            var joystickObject = new GameObject("Joystick", typeof(RectTransform), typeof(JoystickView));
            var joystickRect = joystickObject.GetComponent<RectTransform>();
            joystickRect.SetParent(inputRootRect, false);

            _joystickView = joystickObject.GetComponent<JoystickView>();
            _inputReader = inputRoot.GetComponent<PlayerInputReader>();
            _inputReader.BindJoystick(_joystickView);
        }

        private void EnsureRobot(RobotConfig robotConfig)
        {
            if (_robotController != null)
            {
                return;
            }

            if (robotConfig.RobotPrefab == null)
            {
                Debug.LogError("Stage3 bootstrap: RobotConfig has no robot prefab assigned.", robotConfig);
                return;
            }

            var robotObject = Instantiate(robotConfig.RobotPrefab, robotConfig.StartPosition, Quaternion.identity);
            robotObject.name = "Robot";

            _robotController = robotObject.GetComponent<RobotController>();
            if (_robotController == null)
            {
                _robotController = robotObject.AddComponent<RobotController>();
            }
        }

        private void EnsureScoopEjectButton(MainUiProvider mainUiProvider)
        {
            if (_scoopEjectButton == null)
            {
                var buttonObject = new GameObject("ScoopEjectButton", typeof(RectTransform), typeof(Image), typeof(Button));
                var buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.SetParent(mainUiProvider.HudParent, false);
                buttonRect.anchorMin = new Vector2(1f, 0f);
                buttonRect.anchorMax = new Vector2(1f, 0f);
                buttonRect.pivot = new Vector2(1f, 0f);
                buttonRect.anchoredPosition = new Vector2(-42f, 42f);
                buttonRect.sizeDelta = new Vector2(280f, 120f);

                var buttonImage = buttonObject.GetComponent<Image>();
                buttonImage.color = new Color(0.17f, 0.24f, 0.33f, 0.93f);

                _scoopEjectButton = buttonObject.GetComponent<Button>();
                _scoopEjectButton.targetGraphic = buttonImage;

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = "EJECT";
                label.alignment = TextAlignmentOptions.Center;
                label.fontSize = 48f;
                label.color = Color.white;
                if (mainUiProvider.ScoreLabel != null)
                {
                    label.font = mainUiProvider.ScoreLabel.font;
                }
            }

            _scoopEjectButton.onClick.RemoveAllListeners();
            _scoopEjectButton.onClick.AddListener(_robotController.EjectItemsFromScoop);
        }
    }
}
