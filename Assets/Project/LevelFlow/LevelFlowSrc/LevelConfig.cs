using System;
using System.Collections.Generic;
using VacuumSorter.Items;
using UnityEngine;

namespace VacuumSorter.LevelFlow
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "VacuumSorter/LevelFlow/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Serializable]
        public sealed class ItemBatchDefinition
        {
            [SerializeField] private ItemTypeConfig _itemType;
            [SerializeField, Min(1)] private int _count = 10;

            public ItemTypeConfig ItemType => _itemType;
            public int Count => _count;
        }

        [Serializable]
        public sealed class TargetDefinition
        {
            [SerializeField] private ItemTypeConfig _itemType;
            [SerializeField, Min(1)] private int _requiredCount = 10;
            [SerializeField, Min(0)] private int _anchorIndex;

            public ItemTypeConfig ItemType => _itemType;
            public int RequiredCount => _requiredCount;
            public int AnchorIndex => _anchorIndex;
        }

        [Serializable]
        public sealed class SpawnSettings
        {
            [SerializeField, Min(0f)] private float _spawnRadius = 0.95f;
            [SerializeField, Min(1f)] private float _spawnHeight = 6f;
            [SerializeField, Min(0.05f)] private float _heightStep = 0.22f;
            [SerializeField, Min(0f)] private float _spawnInterval = 0.03f;

            public float SpawnRadius => _spawnRadius;
            public float SpawnHeight => _spawnHeight;
            public float HeightStep => _heightStep;
            public float SpawnInterval => _spawnInterval;
        }

        [Serializable]
        public sealed class TargetInteractionSettings
        {
            [SerializeField, Min(0.05f)] private float _acceptRadius = 0.85f;
            [SerializeField, Min(0.05f)] private float _acceptHeight = 0.8f;
            [SerializeField, Min(0.05f)] private float _pullDuration = 0.24f;
            [SerializeField, Min(0.05f)] private float _sinkDepth = 0.55f;
            [SerializeField, Min(0f)] private float _wrongItemRejectImpulse = 1.6f;

            public float AcceptRadius => _acceptRadius;
            public float AcceptHeight => _acceptHeight;
            public float PullDuration => _pullDuration;
            public float SinkDepth => _sinkDepth;
            public float WrongItemRejectImpulse => _wrongItemRejectImpulse;
        }

        [Serializable]
        public sealed class ArenaLayoutSettings
        {
            [SerializeField, Min(8f)] private float _arenaWidth = 16f;
            [SerializeField, Min(8f)] private float _arenaDepth = 16f;
            [SerializeField, Min(1f)] private float _centerZoneDiameter = 4f;
            [SerializeField, Min(2f)] private float _anchorRadius = 5.2f;
            [SerializeField, Min(2)] private int _anchorCount = 6;

            public float ArenaWidth => _arenaWidth;
            public float ArenaDepth => _arenaDepth;
            public float CenterZoneDiameter => _centerZoneDiameter;
            public float AnchorRadius => _anchorRadius;
            public int AnchorCount => _anchorCount;
        }

        [Header("Identity")]
        [SerializeField, Min(1)] private int _levelNumber = 1;

        [Header("Spawn content")]
        [SerializeField] private List<ItemBatchDefinition> _itemBatches = new();
        [SerializeField] private SpawnSettings _spawn = new();

        [Header("Target content")]
        [SerializeField] private List<TargetDefinition> _targets = new();
        [SerializeField] private TargetInteractionSettings _targetInteraction = new();

        [Header("Layout")]
        [SerializeField] private ArenaLayoutSettings _arenaLayout = new();

        public int LevelNumber => _levelNumber;
        public IReadOnlyList<ItemBatchDefinition> ItemBatches => _itemBatches;
        public SpawnSettings Spawn => _spawn;
        public IReadOnlyList<TargetDefinition> Targets => _targets;
        public TargetInteractionSettings TargetInteraction => _targetInteraction;
        public ArenaLayoutSettings ArenaLayout => _arenaLayout;
    }
}
