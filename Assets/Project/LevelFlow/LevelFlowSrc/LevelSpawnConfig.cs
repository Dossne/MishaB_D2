using System.Collections.Generic;
using UnityEngine;
using VacuumSorter.Items;

namespace VacuumSorter.LevelFlow
{
    [CreateAssetMenu(fileName = "LevelSpawnConfig", menuName = "VacuumSorter/LevelFlow/Level Spawn Config")]
    public sealed class LevelSpawnConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _totalItemCount = 44;
        [SerializeField, Min(0f)] private float _spawnRadius = 0.95f;
        [SerializeField, Min(1f)] private float _spawnHeight = 6f;
        [SerializeField, Min(0.05f)] private float _heightStep = 0.22f;
        [SerializeField, Min(0f)] private float _spawnInterval = 0.03f;
        [SerializeField] private List<ItemTypeConfig> _itemTypes = new();

        public int TotalItemCount => _totalItemCount;
        public float SpawnRadius => _spawnRadius;
        public float SpawnHeight => _spawnHeight;
        public float HeightStep => _heightStep;
        public float SpawnInterval => _spawnInterval;
        public IReadOnlyList<ItemTypeConfig> ItemTypes => _itemTypes;
    }
}