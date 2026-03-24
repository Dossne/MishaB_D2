using System;
using UnityEngine;

namespace VacuumSorter.Feedback
{
    [CreateAssetMenu(fileName = "FeedbackConfig", menuName = "VacuumSorter/Feedback/Feedback Config")]
    public sealed class FeedbackConfig : ScriptableObject
    {
        [Serializable]
        public sealed class FloatingTextSettings
        {
            [SerializeField, Min(0.2f)] private float _lifetime = 0.85f;
            [SerializeField, Min(0f)] private float _riseDistance = 110f;
            [SerializeField, Min(0.01f)] private float _startScale = 1f;
            [SerializeField, Min(0.01f)] private float _endScale = 0.82f;
            [SerializeField, Min(12f)] private float _fontSize = 66f;

            public float Lifetime => _lifetime;
            public float RiseDistance => _riseDistance;
            public float StartScale => _startScale;
            public float EndScale => _endScale;
            public float FontSize => _fontSize;
        }

        [Serializable]
        public sealed class SortSuccessSettings
        {
            [SerializeField] private bool _enabled = true;
            [SerializeField] private string _floatingText = "+1";
            [SerializeField] private bool _useItemColorForText = false;
            [SerializeField] private Color _textColor = Color.white;
            [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 0.42f, 0f);
            [SerializeField] private Color _glowColor = new Color(0.58f, 1f, 0.64f, 0.9f);
            [SerializeField, Min(0.05f)] private float _glowDuration = 0.22f;
            [SerializeField, Min(0.05f)] private float _glowStartScale = 0.35f;
            [SerializeField, Min(0.05f)] private float _glowEndScale = 1.1f;
            [SerializeField] private bool _triggerHaptic = true;

            public bool Enabled => _enabled;
            public string FloatingText => _floatingText;
            public bool UseItemColorForText => _useItemColorForText;
            public Color TextColor => _textColor;
            public Vector3 WorldOffset => _worldOffset;
            public Color GlowColor => _glowColor;
            public float GlowDuration => _glowDuration;
            public float GlowStartScale => _glowStartScale;
            public float GlowEndScale => _glowEndScale;
            public bool TriggerHaptic => _triggerHaptic;
        }

        [Serializable]
        public sealed class DensePushSettings
        {
            [SerializeField] private bool _enabled = true;
            [SerializeField, Min(0.05f)] private float _cooldown = 0.2f;
            [SerializeField, Min(0f)] private float _minRobotSpeed = 1.35f;
            [SerializeField, Min(1)] private int _minNearbyDynamicBodies = 4;
            [SerializeField] private Color _glowColor = new Color(1f, 0.79f, 0.42f, 0.55f);
            [SerializeField, Min(0.05f)] private float _glowDuration = 0.12f;
            [SerializeField, Min(0.05f)] private float _glowStartScale = 0.4f;
            [SerializeField, Min(0.05f)] private float _glowEndScale = 0.95f;
            [SerializeField] private bool _triggerHaptic;

            public bool Enabled => _enabled;
            public float Cooldown => _cooldown;
            public float MinRobotSpeed => _minRobotSpeed;
            public int MinNearbyDynamicBodies => _minNearbyDynamicBodies;
            public Color GlowColor => _glowColor;
            public float GlowDuration => _glowDuration;
            public float GlowStartScale => _glowStartScale;
            public float GlowEndScale => _glowEndScale;
            public bool TriggerHaptic => _triggerHaptic;
        }

        [Serializable]
        public sealed class AudioSettings
        {
            [SerializeField] private bool _enableAudioHooks = true;
            [SerializeField] private bool _logMissingAudioClips = true;
            [SerializeField] private AudioClip _sortSuccessClip;
            [SerializeField, Range(0f, 1f)] private float _sortSuccessVolume = 0.75f;
            [SerializeField, Min(0.1f)] private float _sortSuccessPitch = 1f;
            [SerializeField] private AudioClip _densePushClip;
            [SerializeField, Range(0f, 1f)] private float _densePushVolume = 0.32f;
            [SerializeField, Min(0.1f)] private float _densePushPitch = 1f;

            public bool EnableAudioHooks => _enableAudioHooks;
            public bool LogMissingAudioClips => _logMissingAudioClips;
            public AudioClip SortSuccessClip => _sortSuccessClip;
            public float SortSuccessVolume => _sortSuccessVolume;
            public float SortSuccessPitch => _sortSuccessPitch;
            public AudioClip DensePushClip => _densePushClip;
            public float DensePushVolume => _densePushVolume;
            public float DensePushPitch => _densePushPitch;
        }

        [Serializable]
        public sealed class HapticSettings
        {
            [SerializeField] private bool _enabled = true;
            [SerializeField] private bool _vibrateOnSortSuccess = true;
            [SerializeField] private bool _vibrateOnDensePush;
            [SerializeField, Min(0f)] private float _vibrateCooldown = 0.12f;
            [SerializeField] private bool _logInEditor = true;

            public bool Enabled => _enabled;
            public bool VibrateOnSortSuccess => _vibrateOnSortSuccess;
            public bool VibrateOnDensePush => _vibrateOnDensePush;
            public float VibrateCooldown => _vibrateCooldown;
            public bool LogInEditor => _logInEditor;
        }

        [Header("Floating Text")]
        [SerializeField] private FloatingTextSettings _floatingText = new FloatingTextSettings();

        [Header("Successful Sort")]
        [SerializeField] private SortSuccessSettings _sortSuccess = new SortSuccessSettings();

        [Header("Dense Pile Push")]
        [SerializeField] private DensePushSettings _densePush = new DensePushSettings();

        [Header("Audio")]
        [SerializeField] private AudioSettings _audio = new AudioSettings();

        [Header("Haptic")]
        [SerializeField] private HapticSettings _haptic = new HapticSettings();

        public FloatingTextSettings FloatingText => _floatingText;
        public SortSuccessSettings SortSuccess => _sortSuccess;
        public DensePushSettings DensePush => _densePush;
        public AudioSettings Audio => _audio;
        public HapticSettings Haptic => _haptic;
    }
}

