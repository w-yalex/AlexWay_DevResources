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

        [SerializeField] private Settings _settings;

        private static readonly object _instanceFeedbackTarget = new();

        private static CinemachineBrain _cinemachineBrain;
        private static CinemachineImpulseSource _impulseSource;

        [Serializable]
        public struct Settings
        {
            [Header("Intensity")]
            public NoiseSettings NoiseSettings;
            public float ShakeAmplitude;
            public float ShakeFrequency;

            [Header("Timing")]
            public float AttackTime;
            public float SustainTime;
            public float DecayTime;
            public bool IgnoreTimeScale;
        }


        public override void PlayOnObject(Action onComplete = null)
            => PlayInstance(_settings, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Settings customSettings)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Settings).FullName}");
                return;
            }

            PlayInstance(customSettings, onComplete);
        }


        public override void ClearOnObject() => ClearInstance();

        public static void PlayInstance(Settings settings, Action onComplete = null)
            => BeginCinemachineCameraShake(settings, onComplete);


        public static void ClearInstance() => ClearCinemachineCameraShake();

        private static void BeginCinemachineCameraShake(Settings settings, Action onComplete = null)
        {            
            DOTween.Kill(_instanceFeedbackTarget);
            CinemachineImpulseManager.Instance.Clear();

            ValidateCinemachineBrain();
            TrySetCinemachineImpulseListener();
            TrySpawnCinemachineImpulseSource();

            ApplyCinemachineCameraShakeSettings(settings);   

            _impulseSource.GenerateImpulse(); // Sends to listening active camera
            float totalDuration = settings.AttackTime + settings.SustainTime + settings.DecayTime;

            DOVirtual.DelayedCall(totalDuration, () => OnCinemachineCameraShakeComplete(settings, onComplete))
                .SetTarget(_instanceFeedbackTarget);
        }

        
        private static void ClearCinemachineCameraShake()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            CinemachineImpulseManager.Instance.Clear();

            if (_cinemachineBrain) _cinemachineBrain.IgnoreTimeScale = false;
        }


        private static void OnCinemachineCameraShakeComplete(Settings settings, Action onComplete = null)
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


        private static void ApplyCinemachineCameraShakeSettings(Settings settings)
        {            
            var impulseDefinition = _impulseSource.ImpulseDefinition;
            _impulseSource.ImpulseDefinition.RawSignal = settings.NoiseSettings;

            impulseDefinition.AmplitudeGain = settings.ShakeAmplitude;
            impulseDefinition.FrequencyGain = settings.ShakeFrequency;

            impulseDefinition.TimeEnvelope.AttackTime = settings.AttackTime;
            impulseDefinition.TimeEnvelope.SustainTime = settings.SustainTime;
            impulseDefinition.TimeEnvelope.DecayTime = settings.DecayTime;

            _cinemachineBrain.IgnoreTimeScale = settings.IgnoreTimeScale;
        }

 
    }
}
