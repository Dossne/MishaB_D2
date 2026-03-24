using System.Collections.Generic;
using UnityEngine;

namespace VacuumSorter.LevelFlow
{
    [CreateAssetMenu(fileName = "LevelCatalogConfig", menuName = "VacuumSorter/LevelFlow/Level Catalog Config")]
    public sealed class LevelCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels = new();

        public IReadOnlyList<LevelConfig> Levels => _levels;
        public int LevelCount => _levels != null ? _levels.Count : 0;

        public bool TryGetLevelByIndex(int levelIndex, out LevelConfig levelConfig)
        {
            levelConfig = null;
            if (_levels == null || _levels.Count == 0)
            {
                return false;
            }

            var wrappedIndex = levelIndex;
            if (wrappedIndex < 0)
            {
                wrappedIndex = 0;
            }

            wrappedIndex %= _levels.Count;
            levelConfig = _levels[wrappedIndex];
            return levelConfig != null;
        }
    }
}
