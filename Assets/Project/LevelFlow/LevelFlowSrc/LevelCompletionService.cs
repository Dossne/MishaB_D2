using System;
using VacuumSorter.Items;
using VacuumSorter.Scoring;

namespace VacuumSorter.LevelFlow
{
    public sealed class LevelCompletionService
    {
        private readonly ScoreService _scoreService;

        public event Action ProgressChanged;
        public event Action Completed;

        public bool IsCompleted { get; private set; }
        public int Score => _scoreService != null ? _scoreService.Score : 0;
        public int RemainingRequired => _scoreService != null ? _scoreService.RemainingRequired : 0;
        public int TotalRequired => _scoreService != null ? _scoreService.TotalRequired : 0;
        public int TotalSorted => _scoreService != null ? _scoreService.TotalSorted : 0;

        public LevelCompletionService(ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        public bool TryRegisterSorted(ItemTypeConfig itemType)
        {
            if (IsCompleted || _scoreService == null)
            {
                return false;
            }

            if (!_scoreService.TryRegisterSorted(itemType))
            {
                return false;
            }

            ProgressChanged?.Invoke();

            if (_scoreService.IsComplete)
            {
                IsCompleted = true;
                Completed?.Invoke();
            }

            return true;
        }
    }
}
