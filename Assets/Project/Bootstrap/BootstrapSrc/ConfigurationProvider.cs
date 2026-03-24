using System.Collections.Generic;
using UnityEngine;

namespace VacuumSorter.Bootstrap
{
    [CreateAssetMenu(
        fileName = "ConfigurationProvider",
        menuName = "VacuumSorter/Bootstrap/Configuration Provider")]
    public sealed class ConfigurationProvider : ScriptableObject
    {
        // Runtime systems must read gameplay assets from these serialized references.
        // Do not fetch gameplay assets via Unity Editor-only APIs (e.g. AssetDatabase).
        [Header("Runtime config assets")]
        [SerializeField] private List<ScriptableObject> _gameplayConfigs = new();

        [Header("Runtime gameplay assets")]
        [SerializeField] private List<Object> _gameplayAssets = new();

        public IReadOnlyList<ScriptableObject> GameplayConfigs => _gameplayConfigs;
        public IReadOnlyList<Object> GameplayAssets => _gameplayAssets;

        public bool TryGetConfig<TConfig>(out TConfig config) where TConfig : ScriptableObject
        {
            for (var i = 0; i < _gameplayConfigs.Count; i++)
            {
                if (_gameplayConfigs[i] is TConfig typed)
                {
                    config = typed;
                    return true;
                }
            }

            config = null;
            return false;
        }
    }
}

