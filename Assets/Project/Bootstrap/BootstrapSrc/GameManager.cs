using System.Collections.Generic;
using UnityEngine;

namespace VacuumSorter.Bootstrap
{
    public interface IRuntimeSystem
    {
        void Initialize(ServiceLocator services);
        void Tick(float deltaTime);
        void Shutdown();
    }

    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private ServiceLocator _serviceLocator;

        private readonly List<IRuntimeSystem> _runtimeSystems = new();
        private bool _isBootstrapped;

        private void Awake()
        {
            Bootstrap();
        }

        private void Update()
        {
            if (!_isBootstrapped)
            {
                return;
            }

            for (var i = 0; i < _runtimeSystems.Count; i++)
            {
                _runtimeSystems[i].Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void RegisterRuntimeSystem(IRuntimeSystem runtimeSystem)
        {
            if (runtimeSystem == null)
            {
                Debug.LogError("GameManager: runtimeSystem is null.", this);
                return;
            }

            _runtimeSystems.Add(runtimeSystem);

            if (_isBootstrapped)
            {
                runtimeSystem.Initialize(_serviceLocator);
            }
        }

        private void Bootstrap()
        {
            if (_isBootstrapped)
            {
                return;
            }

            if (_serviceLocator == null)
            {
                Debug.LogError("GameManager: ServiceLocator reference is missing.", this);
                return;
            }

            _serviceLocator.RegisterGameManager(this);

            var configurationProvider = _serviceLocator.SerializedConfigurationProvider;
            if (configurationProvider == null)
            {
                Debug.LogError("GameManager: ConfigurationProvider reference is missing in ServiceLocator.", _serviceLocator);
                return;
            }

            _serviceLocator.RegisterConfigurationProvider(configurationProvider);

            var mainUiProvider = _serviceLocator.SerializedMainUiProvider;
            if (mainUiProvider == null)
            {
                Debug.LogError("GameManager: MainUiProvider reference is missing in ServiceLocator.", _serviceLocator);
                return;
            }

            _serviceLocator.RegisterMainUiProvider(mainUiProvider);

            for (var i = 0; i < _runtimeSystems.Count; i++)
            {
                _runtimeSystems[i].Initialize(_serviceLocator);
            }

            _isBootstrapped = true;
            Debug.Log("GameManager: bootstrap complete.");
        }

        private void Shutdown()
        {
            if (!_isBootstrapped)
            {
                return;
            }

            for (var i = _runtimeSystems.Count - 1; i >= 0; i--)
            {
                _runtimeSystems[i].Shutdown();
            }

            _isBootstrapped = false;
            Debug.Log("GameManager: shutdown complete.");
        }
    }
}
