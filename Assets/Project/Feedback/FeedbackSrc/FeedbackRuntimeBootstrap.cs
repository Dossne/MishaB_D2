using System.Collections;
using VacuumSorter.Bootstrap;
using VacuumSorter.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacuumSorter.Feedback
{
    [DisallowMultipleComponent]
    public sealed class FeedbackRuntimeBootstrap : MonoBehaviour
    {
        private const string RuntimeRootName = "Stage8FeedbackRuntime";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private FloatingTextPresenter _floatingTextPresenter;

        private ServiceLocator _services;
        private FeedbackConfig _feedbackConfig;
        private IAudioFeedbackHook _audioHook;
        private IHapticFeedbackHook _hapticHook;
        private bool _isInitialized;
        private bool _hasLoggedMissingConfig;
        private float _nextDensePushTime;

        public static FeedbackRuntimeBootstrap Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBootstrap()
        {
            var host = GameObject.Find(RuntimeRootName);
            if (host == null)
            {
                host = new GameObject(RuntimeRootName);
                DontDestroyOnLoad(host);
            }

            var bootstrap = host.GetComponent<FeedbackRuntimeBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<FeedbackRuntimeBootstrap>();
            }

            bootstrap.InitializeIfReady();
        }

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            InitializeIfReady();
        }

        private void Update()
        {
            InitializeIfReady();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isInitialized = false;
            _services = null;
            InitializeIfReady();
        }

        private void InitializeIfReady()
        {
            if (_isInitialized)
            {
                return;
            }

            var services = ServiceLocator.Current;
            if (services == null || services.MainUiProvider == null || services.ConfigurationProvider == null)
            {
                return;
            }

            FeedbackConfig feedbackConfig;
            if (!services.ConfigurationProvider.TryGetConfig(out feedbackConfig) || feedbackConfig == null)
            {
                if (!_hasLoggedMissingConfig)
                {
                    Debug.LogWarning("Stage8 feedback: FeedbackConfig is missing in ConfigurationProvider.", services.ConfigurationProvider);
                    _hasLoggedMissingConfig = true;
                }

                return;
            }

            _services = services;
            _feedbackConfig = feedbackConfig;

            if (_floatingTextPresenter == null)
            {
                _floatingTextPresenter = GetComponent<FloatingTextPresenter>();
            }

            if (_floatingTextPresenter == null)
            {
                _floatingTextPresenter = gameObject.AddComponent<FloatingTextPresenter>();
            }

            _floatingTextPresenter.Initialize(_services.MainUiProvider, _feedbackConfig);

            _audioHook = new AudioFeedbackHook();
            _hapticHook = new HapticFeedbackHook();

            _isInitialized = true;
            _hasLoggedMissingConfig = false;

            Debug.Log("Stage8 feedback: runtime hooks initialized.");
        }

        public void PlaySortSuccess(ItemTypeConfig itemType, Vector3 worldPosition)
        {
            if (!_isInitialized || _feedbackConfig == null)
            {
                return;
            }

            var settings = _feedbackConfig.SortSuccess;
            if (settings == null || !settings.Enabled)
            {
                return;
            }

            var textValue = string.IsNullOrWhiteSpace(settings.FloatingText) ? "+1" : settings.FloatingText;
            var textColor = Color.white;

            _floatingTextPresenter.ShowWorldText(textValue, worldPosition + settings.WorldOffset, textColor);
            StartCoroutine(SpawnGlowPulse(worldPosition, settings.GlowColor, settings.GlowDuration, settings.GlowStartScale, settings.GlowEndScale));

            _audioHook?.Play(
                _feedbackConfig.Audio,
                _feedbackConfig.Audio.SortSuccessClip,
                "sort_success",
                worldPosition,
                _feedbackConfig.Audio.SortSuccessVolume,
                _feedbackConfig.Audio.SortSuccessPitch);

            if (settings.TriggerHaptic)
            {
                _hapticHook?.TriggerSortSuccess(_feedbackConfig.Haptic);
            }
        }

        public void TryPlayDensePush(Vector3 worldPosition, float robotSpeed, int nearbyDynamicBodies)
        {
            if (!_isInitialized || _feedbackConfig == null)
            {
                return;
            }

            var settings = _feedbackConfig.DensePush;
            if (settings == null || !settings.Enabled)
            {
                return;
            }

            if (Time.time < _nextDensePushTime)
            {
                return;
            }

            if (robotSpeed < settings.MinRobotSpeed || nearbyDynamicBodies < settings.MinNearbyDynamicBodies)
            {
                return;
            }

            _nextDensePushTime = Time.time + settings.Cooldown;

            StartCoroutine(SpawnGlowPulse(worldPosition, settings.GlowColor, settings.GlowDuration, settings.GlowStartScale, settings.GlowEndScale));

            _audioHook?.Play(
                _feedbackConfig.Audio,
                _feedbackConfig.Audio.DensePushClip,
                "dense_push",
                worldPosition,
                _feedbackConfig.Audio.DensePushVolume,
                _feedbackConfig.Audio.DensePushPitch);

            if (settings.TriggerHaptic)
            {
                _hapticHook?.TriggerDensePush(_feedbackConfig.Haptic);
            }
        }

        private static IEnumerator SpawnGlowPulse(
            Vector3 worldPosition,
            Color color,
            float duration,
            float startScale,
            float endScale)
        {
            var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "FeedbackPulse";
            pulse.transform.position = worldPosition;
            pulse.transform.rotation = Quaternion.identity;

            var collider = pulse.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = pulse.GetComponent<Renderer>();
            var propertyBlock = new MaterialPropertyBlock();

            var elapsed = 0f;
            var safeDuration = Mathf.Max(0.01f, duration);

            while (elapsed < safeDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / safeDuration);
                var eased = 1f - Mathf.Pow(1f - t, 2f);

                pulse.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);

                if (renderer != null)
                {
                    var frameColor = color;
                    frameColor.a = Mathf.Lerp(color.a, 0f, eased);
                    renderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetColor(BaseColorId, frameColor);
                    propertyBlock.SetColor(ColorId, frameColor);
                    renderer.SetPropertyBlock(propertyBlock);
                }

                yield return null;
            }

            Destroy(pulse);
        }

        private interface IAudioFeedbackHook
        {
            void Play(FeedbackConfig.AudioSettings settings, AudioClip clip, string eventId, Vector3 worldPosition, float volume, float pitch);
        }

        private sealed class AudioFeedbackHook : IAudioFeedbackHook
        {
            public void Play(FeedbackConfig.AudioSettings settings, AudioClip clip, string eventId, Vector3 worldPosition, float volume, float pitch)
            {
                if (settings == null || !settings.EnableAudioHooks)
                {
                    return;
                }

                if (clip == null)
                {
                    if (settings.LogMissingAudioClips)
                    {
                        Debug.Log($"Stage8 feedback audio hook: placeholder for '{eventId}' (clip not assigned).");
                    }

                    return;
                }

                var sourceObject = new GameObject($"AudioHook_{eventId}");
                sourceObject.transform.position = worldPosition;

                var source = sourceObject.AddComponent<AudioSource>();
                source.spatialBlend = 0f;
                source.playOnAwake = false;
                source.clip = clip;
                source.volume = Mathf.Clamp01(volume);
                source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
                source.Play();

                var lifetime = clip.length / Mathf.Max(0.1f, source.pitch);
                Destroy(sourceObject, lifetime + 0.05f);
            }
        }

        private interface IHapticFeedbackHook
        {
            void TriggerSortSuccess(FeedbackConfig.HapticSettings settings);
            void TriggerDensePush(FeedbackConfig.HapticSettings settings);
        }

        private sealed class HapticFeedbackHook : IHapticFeedbackHook
        {
            private float _nextVibrateTime;

            public void TriggerSortSuccess(FeedbackConfig.HapticSettings settings)
            {
                Trigger(settings, settings != null && settings.VibrateOnSortSuccess, "sort_success");
            }

            public void TriggerDensePush(FeedbackConfig.HapticSettings settings)
            {
                Trigger(settings, settings != null && settings.VibrateOnDensePush, "dense_push");
            }

            private void Trigger(FeedbackConfig.HapticSettings settings, bool shouldVibrate, string eventId)
            {
                if (settings == null || !settings.Enabled || !shouldVibrate)
                {
                    return;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
                if (Time.unscaledTime >= _nextVibrateTime)
                {
                    _nextVibrateTime = Time.unscaledTime + settings.VibrateCooldown;
                    TriggerAndroidVibration();
                }
#else
                if (settings.LogInEditor)
                {
                    Debug.Log($"Stage8 haptic hook: placeholder for '{eventId}'.");
                }
#endif
            }

            private static void TriggerAndroidVibration()
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator == null)
                    {
                        return;
                    }

                    vibrator.Call("vibrate", 20L);
                }
            }
        }
    }
}





