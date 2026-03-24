using System;
using UnityEngine;

namespace VacuumSorter.Progression
{
    public sealed class UpgradeService
    {
        private readonly UpgradeConfig _config;

        public event Action UpgradeChanged;

        public int TotalCollectedScore { get; private set; }
        public int CurrentScoopLevel => _config != null ? _config.GetScoopLevelForScore(TotalCollectedScore) : 0;
        public float CurrentScoopMultiplier => _config != null ? _config.GetScoopSizeMultiplierForScore(TotalCollectedScore) : 1f;

        public UpgradeService(UpgradeConfig config)
        {
            _config = config;
            TotalCollectedScore = config != null ? config.StartCollectedScore : 0;
        }

        public void AddCollectedScore(int amount)
        {
            if (_config == null || amount <= 0)
            {
                return;
            }

            var previousLevel = CurrentScoopLevel;
            var previousMultiplier = CurrentScoopMultiplier;

            TotalCollectedScore += amount;

            var levelChanged = CurrentScoopLevel != previousLevel;
            var multiplierChanged = Mathf.Abs(CurrentScoopMultiplier - previousMultiplier) > 0.0001f;
            if (levelChanged || multiplierChanged)
            {
                UpgradeChanged?.Invoke();
            }
        }
    }
}
