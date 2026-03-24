using UnityEngine;

namespace VacuumSorter.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class ServiceLocator : MonoBehaviour
    {
        [SerializeField] private ConfigurationProvider _configurationProvider;

        public static ServiceLocator Current { get; private set; }

        public GameManager GameManager { get; private set; }
        public ConfigurationProvider ConfigurationProvider { get; private set; }
        public ConfigurationProvider SerializedConfigurationProvider => _configurationProvider;

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Debug.LogWarning("ServiceLocator: replacing previous Current instance.", this);
            }

            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public void RegisterGameManager(GameManager gameManager)
        {
            GameManager = gameManager;
            Debug.Log("ServiceLocator: registered GameManager (step 1).");
        }

        public void RegisterConfigurationProvider(ConfigurationProvider configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
            Debug.Log("ServiceLocator: registered ConfigurationProvider (step 2).");
        }
    }
}
