using UnityEngine;

namespace VacuumSorter.Progression
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "VacuumSorter/Progression/Upgrade Config")]
    public sealed class UpgradeConfig : ScriptableObject
    {
        [Header("Score-driven scoop growth")]
        [SerializeField, Min(0)] private int _startCollectedScore;
        [SerializeField, Min(1f)] private float _baseScoopSizeMultiplier = 1f;
        [SerializeField, Min(0.01f)] private float _widthMultiplierPerLevel = 0.21f;
        [SerializeField, Min(0)] private int _level0To1Score = 5;
        [SerializeField, Min(0)] private int _maxScoopLevel = 12;

        public int StartCollectedScore => Mathf.Max(0, _startCollectedScore);
        public int MaxScoopLevel => _maxScoopLevel;

        public int ClampScoopLevel(int level)
        {
            return Mathf.Clamp(level, 0, _maxScoopLevel);
        }

        public int GetScoopLevelForScore(int totalCollectedScore)
        {
            var clampedScore = Mathf.Max(0, totalCollectedScore);
            var level = 0;

            for (var nextLevel = 1; nextLevel <= _maxScoopLevel; nextLevel++)
            {
                if (clampedScore < GetScoreRequiredForLevel(nextLevel))
                {
                    break;
                }

                level = nextLevel;
            }

            return level;
        }

        public float GetScoopSizeMultiplierForScore(int totalCollectedScore)
        {
            var level = GetScoopLevelForScore(totalCollectedScore);
            return _baseScoopSizeMultiplier + level * _widthMultiplierPerLevel;
        }

        // Level score requirements follow 5, 15, 30, ... where each next step is harder.
        private int GetScoreRequiredForLevel(int targetLevel)
        {
            var level = Mathf.Max(1, targetLevel);
            var triangular = level * (level + 1) / 2;
            return _level0To1Score * triangular;
        }
    }
}


