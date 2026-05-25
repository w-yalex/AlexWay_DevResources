using UnityEngine;
using Unity.Cinemachine;
using System;
using DG.Tweening;

namespace AW.UnityResources
{
    /// <summary>
    /// Uses CinmeachineImpulseSource for Camera Shake
    /// </summary>
    public class CinemachineCameraShake : JuiceComponent
    {
        [Header("Cinemachine Camera Shake")]
        [SerializeField] private Config _defaultConfig;

        private static readonly object _instanceFeedbackTarget = new();

        private static CinemachineBrain _cinemachineBrain;
        private static CinemachineImpulseSource _impulseSource;

        [Serializable]
        public class Config
        {
            [Header("Intensity")]
            public NoiseSettings NoiseSettings;
            public float ShakeAmplitude = 3.5f;
            public float ShakeFrequency = 3.5f;

            [Header("Timing")]
            public float AttackTime = 0.1f;
            public float SustainTime = 0.2f;
            public float DecayTime = 0.1f;
            public bool IgnoreTimeScale;
        }


        private void Awake()
        {
            if (!_defaultConfig.NoiseSettings)
            {
                UnityDebug.LogWarning(this, $"No Noise Settings configured in default config for {gameObject.name} gameObject");
            }
        }


        public override void PlayOnObject(Action onComplete = null)
            => PlayInstance(_defaultConfig, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Config customConfig)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Config).FullName}");
                return;
            }

            PlayInstance(customConfig, onComplete);
        }


        public override void ClearOnObject() => ClearInstance();

        public static void PlayInstance(Config config, Action onComplete = null)
            => BeginCinemachineCameraShake(config, onComplete);


        public static void ClearInstance() => ClearCinemachineCameraShake();

        private static void BeginCinemachineCameraShake(Config config, Action onComplete = null)
        {            
            DOTween.Kill(_instanceFeedbackTarget);
            CinemachineImpulseManager.Instance.Clear();

            ValidateCinemachineBrain();
            TrySetCinemachineImpulseListener();
            TrySpawnCinemachineImpulseSource();

            ApplyCinemachineCameraShakeConfig(config);   

            _impulseSource.GenerateImpulse(); // Sends to listening active camera
            float totalDuration = config.AttackTime + config.SustainTime + config.DecayTime;

            DOVirtual.DelayedCall(totalDuration, () => OnCinemachineCameraShakeComplete(config, onComplete))
                .SetTarget(_instanceFeedbackTarget);
        }

        
        private static void ClearCinemachineCameraShake()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            CinemachineImpulseManager.Instance.Clear();

            if (_cinemachineBrain) _cinemachineBrain.IgnoreTimeScale = false;
        }


        private static void OnCinemachineCameraShakeComplete(Config config, Action onComplete = null)
        {
            CinemachineImpulseManager.Instance.Clear();
            _cinemachineBrain.IgnoreTimeScale = false;
            
            onComplete?.Invoke();
        }


        private static void ValidateCinemachineBrain()
        {
            if (_cinemachineBrain) return;

            if (!Camera.main.TryGetComponent(out CinemachineBrain cinemachineBrain))
            {
                UnityDebug.LogError(typeof(CinemachineCameraShake), $"No Cinemachine brain found on Camera.Main");
                return;
            }

            _cinemachineBrain = cinemachineBrain;
        }


        private static bool TrySetCinemachineImpulseListener()
        { 
            CinemachineCamera activeCam = _cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
            if (!activeCam || activeCam.TryGetComponent<CinemachineImpulseListener>(out _)) return false;
    
            var impulseListener = activeCam.gameObject.AddComponent<CinemachineImpulseListener>();

            impulseListener.ApplyAfter = CinemachineCore.Stage.Noise;
            impulseListener.ChannelMask = 1 << 0; // Default channel mask
            impulseListener.Gain = 1; // Uses exact definition values set in the ImpulseSoure

            UnityDebug.Log(typeof(CinemachineCameraShake), $"Impulse listener has been successfully added to {activeCam.gameObject.name}");
            return true;
  
        }


        private static bool TrySpawnCinemachineImpulseSource()
        {
            if (_impulseSource) return false;

            var newImpulseSourceObj = new GameObject("CinemachineCameraShake_ImpulseSource");
            _impulseSource = newImpulseSourceObj.AddComponent<CinemachineImpulseSource>();

            var impulseDefinition = _impulseSource.ImpulseDefinition;
            impulseDefinition.ImpulseChannel = 1 << 0;  // Default channel mask to match the listener
            impulseDefinition.TimeEnvelope.ScaleWithImpact = false;

            UnityDebug.Log(typeof(CinemachineCameraShake), "Impulse source has successfully been spawned");
            return true;
        }


        private static void ApplyCinemachineCameraShakeConfig(Config config)
        {            
            var impulseDefinition = _impulseSource.ImpulseDefinition;

            _impulseSource.ImpulseDefinition.RawSignal = config.NoiseSettings;

            impulseDefinition.AmplitudeGain = config.ShakeAmplitude;
            impulseDefinition.FrequencyGain = config.ShakeFrequency;

            impulseDefinition.TimeEnvelope.AttackTime = config.AttackTime;
            impulseDefinition.TimeEnvelope.SustainTime = config.SustainTime;
            impulseDefinition.TimeEnvelope.DecayTime = config.DecayTime;

            _cinemachineBrain.IgnoreTimeScale = config.IgnoreTimeScale;
        }

 
    }
}
