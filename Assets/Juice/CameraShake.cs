using UnityEngine;
using Unity.Cinemachine;
using System;
using DG.Tweening;

namespace AW.UnityResources
{
    public class CameraShake : JuiceComponent
    {
        [SerializeField] private OnPlay _onPlayData;

        private static readonly object _instancedCameraShakeTarget = new();
        private static CinemachineImpulseSource _impulseSource;

        [Serializable]
        public struct OnPlay
        {
            [Header("Intensity")]

            public NoiseSettings NoiseSettings;
            public float ShakeAmplitude;
            public float ShakeFrequency;

            [Header("Timing")]
            public float AttackTime;
            public float SustainTime;
            public float DecayTime;
        }


        private void Awake()
        {
            TrySpawnImpulseSource();
        }


        public override void PlayOnObject(Action onComplete = null)
            => SetCameraShake(_onPlayData, this, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not OnPlay onPlayData)
            {
                UnityDebug.LogWarning(this, "Juice data received did not match OnPlay");
                return;
            }

            SetCameraShake(onPlayData, this, onComplete);
        }


        public override void StopOnObject()
        {
            DOTween.Kill(this);
            CinemachineImpulseManager.Instance.Clear();
        }


        private static void SetCameraShake(OnPlay onPlayData, object tweenTarget, Action onComplete = null)
        {
            if (!IsImpulseListenerValid()) return;

            DOTween.Kill(tweenTarget);

            TrySpawnImpulseSource();
            CinemachineImpulseManager.Instance.Clear();

            SetImpulseDefinition(onPlayData);
            _impulseSource.GenerateImpulse(); // Sends to listening active camera

            float totalDuration = onPlayData.AttackTime + onPlayData.SustainTime + onPlayData.DecayTime;

            DOVirtual.DelayedCall(totalDuration, () => onComplete?.Invoke())
                .SetTarget(tweenTarget);
        }

        private static bool IsImpulseListenerValid()
        {
            var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            CinemachineCamera activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;

            if (!activeCam)
            {
                UnityDebug.LogError(typeof(CameraShake), $"No cinemachine camera is controlling the brain, cannot do CameraShake");
                return false;
            }
            
            if (!activeCam.TryGetComponent<CinemachineImpulseListener>(out _))
            {
                var impulseListener =activeCam.gameObject.AddComponent<CinemachineImpulseListener>();

                impulseListener.ApplyAfter = CinemachineCore.Stage.Noise;
                impulseListener.ChannelMask = 1 << 0; // Default channel mask
                impulseListener.Gain = 1; // Uses exact definition values set in the ImpulseSoure

                UnityDebug.Log(typeof(CameraShake), $"An impulse listener has been successfully added to {activeCam.gameObject.name}");
            }
            
            return true;
        }


        public static void PlayInstanced(OnPlay onPlayData, Action onComplete = null)
            => SetCameraShake(onPlayData, _instancedCameraShakeTarget, onComplete);


        public static void StopInstanced()
        {
            DOTween.Kill(_instancedCameraShakeTarget);
            CinemachineImpulseManager.Instance.Clear();
        }

        
        private static bool TrySpawnImpulseSource()
        {
            if (_impulseSource) return false;

            var newSourceObject= new GameObject("CameraShake_ImpulseSource");
            _impulseSource = newSourceObject.AddComponent<CinemachineImpulseSource>();

            var impulseDefinition = _impulseSource.ImpulseDefinition;
            impulseDefinition.ImpulseChannel = 1 << 0;  // Default channel mask to match the listener
            impulseDefinition.TimeEnvelope.ScaleWithImpact = false;

            UnityDebug.Log(typeof(CameraShake), "Impulse source has been spawned");
            return true;
        }


        private static void SetImpulseDefinition(OnPlay onPlayData)
        {
            if (!_impulseSource) return;

            var impulseDefinition = _impulseSource.ImpulseDefinition;
            
            _impulseSource.ImpulseDefinition.RawSignal = onPlayData.NoiseSettings;

            impulseDefinition.AmplitudeGain = onPlayData.ShakeAmplitude;
            impulseDefinition.FrequencyGain = onPlayData.ShakeFrequency;

            impulseDefinition.TimeEnvelope.AttackTime = onPlayData.AttackTime;
            impulseDefinition.TimeEnvelope.SustainTime = onPlayData.SustainTime;
            impulseDefinition.TimeEnvelope.DecayTime = onPlayData.DecayTime;
        }


    }
}
