using System;
using System.Collections.Generic;
using VacuumSorter.Items;
using UnityEngine;

namespace VacuumSorter.SortTargets
{
    [CreateAssetMenu(fileName = "SortTargetConfig", menuName = "VacuumSorter/SortTargets/Sort Target Config")]
    public sealed class SortTargetConfig : ScriptableObject
    {
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

        [Header("Target setup")]
        [SerializeField] private List<TargetDefinition> _targets = new();

        [Header("Interaction")]
        [SerializeField, Min(0.05f)] private float _acceptRadius = 0.85f;
        [SerializeField, Min(0.05f)] private float _acceptHeight = 0.8f;
        [SerializeField, Min(0.05f)] private float _pullDuration = 0.24f;
        [SerializeField, Min(0.05f)] private float _sinkDepth = 0.55f;
        [SerializeField, Min(0f)] private float _wrongItemRejectImpulse = 1.6f;

        public IReadOnlyList<TargetDefinition> Targets => _targets;
        public float AcceptRadius => _acceptRadius;
        public float AcceptHeight => _acceptHeight;
        public float PullDuration => _pullDuration;
        public float SinkDepth => _sinkDepth;
        public float WrongItemRejectImpulse => _wrongItemRejectImpulse;
    }
}