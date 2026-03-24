using System.Collections.Generic;
using VacuumSorter.Items;
using VacuumSorter.SortTargets;

namespace VacuumSorter.Scoring
{
    public sealed class ScoreService
    {
        private readonly Dictionary<ItemTypeConfig, int> _requiredByType = new();
        private readonly Dictionary<ItemTypeConfig, int> _sortedByType = new();

        public int TotalRequired { get; private set; }
        public int TotalSorted { get; private set; }
        public int Score => TotalSorted;
        public bool IsComplete => TotalRequired > 0 && TotalSorted >= TotalRequired;

        public ScoreService(IReadOnlyList<SortTargetConfig.TargetDefinition> targetDefinitions)
        {
            if (targetDefinitions == null)
            {
                return;
            }

            for (var i = 0; i < targetDefinitions.Count; i++)
            {
                var definition = targetDefinitions[i];
                if (definition == null || definition.ItemType == null || definition.RequiredCount <= 0)
                {
                    continue;
                }

                if (_requiredByType.TryGetValue(definition.ItemType, out var currentRequired))
                {
                    _requiredByType[definition.ItemType] = currentRequired + definition.RequiredCount;
                }
                else
                {
                    _requiredByType.Add(definition.ItemType, definition.RequiredCount);
                }

                TotalRequired += definition.RequiredCount;
            }
        }

        public bool TryRegisterSorted(ItemTypeConfig itemType)
        {
            if (itemType == null || !_requiredByType.TryGetValue(itemType, out var requiredCount))
            {
                return false;
            }

            _sortedByType.TryGetValue(itemType, out var currentSorted);
            if (currentSorted >= requiredCount)
            {
                return false;
            }

            _sortedByType[itemType] = currentSorted + 1;
            TotalSorted++;
            return true;
        }

        public int GetRemaining(ItemTypeConfig itemType)
        {
            if (itemType == null || !_requiredByType.TryGetValue(itemType, out var requiredCount))
            {
                return 0;
            }

            _sortedByType.TryGetValue(itemType, out var currentSorted);
            return requiredCount - currentSorted;
        }
    }
}