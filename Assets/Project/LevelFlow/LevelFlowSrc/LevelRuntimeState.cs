using UnityEngine;

namespace VacuumSorter.LevelFlow
{
    public static class LevelRuntimeState
    {
        public static int CurrentLevelIndex { get; private set; }

        public static int CurrentLevelNumber => CurrentLevelIndex + 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetOnAppStart()
        {
            CurrentLevelIndex = 0;
        }

        public static void RestartCurrentLevel()
        {
            if (CurrentLevelIndex < 0)
            {
                CurrentLevelIndex = 0;
            }
        }

        public static void AdvanceToNextLevel(int levelCount)
        {
            if (levelCount <= 0)
            {
                CurrentLevelIndex = 0;
                return;
            }

            CurrentLevelIndex = (CurrentLevelIndex + 1) % levelCount;
        }
    }
}
