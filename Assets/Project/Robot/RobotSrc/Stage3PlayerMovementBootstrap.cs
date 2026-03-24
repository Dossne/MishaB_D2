using VacuumSorter.Bootstrap;
using VacuumSorter.MainUI;
using VacuumSorter.PlayerInput;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private bool _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var services = ServiceLocator.Current;
            GameObject host = services != null ? services.gameObject : null;

            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
            }

            var bootstrap = host.GetComponent<Stage3PlayerMovementBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<Stage3PlayerMovementBootstrap>();
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
    }
}