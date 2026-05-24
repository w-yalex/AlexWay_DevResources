using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AW.UnityResources
{
    public class LensDistortionBounce : JuiceComponent
    {
  
        [Header("Lens Distortion Pulse")]
        [SerializeField] private Settings _settings;

        private static LensDistortion _lensDistortion;
        private static object _instanceFeedbackTarget = new();

        [Serializable]
        public struct Settings
        {
            [Header("Intensity")]
            [Range(-1f, 1f)] public float TargetDistortion;

            [Header("Initial")]
            public Ease InitialBounceEase;

            [Header("Oscillation")]
            public int TotalOscillationLoops;
            [Range(0f, 1f)] public float OscillationFalloff;

    
            [Header("Timing")]
            public float InitialBounceMaxAttackTime;
            public float DecayOscillationTime;
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
            => BeginLensDistortionPulse(settings, onComplete);


        public static void ClearInstance() => ClearLensDistortionPulse();

        private static void BeginLensDistortionPulse(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            ValidatePostProcessing();
            TrySpawnLensDistortionPulseSource();

            if (settings.TargetDistortion == 0) return;

            float targetDistortion = Mathf.Clamp(settings.TargetDistortion, -1f, 1f);
            float relativeStart = targetDistortion > 0 ? Mathf.Max(0f, _lensDistortion.intensity.value) : Mathf.Min(0f, _lensDistortion.intensity.value);

            float progressToTarget = Mathf.Clamp01(Mathf.Abs(relativeStart / settings.TargetDistortion));
            float attackTime = Mathf.Lerp(settings.InitialBounceMaxAttackTime, 0f, progressToTarget);
            
            int totalSteps = Mathf.Max(2, settings.TotalOscillationLoops * 2) - 1;
            float oscillateStepDuration = settings.DecayOscillationTime / totalSteps;
            float currentTargetDistortion = targetDistortion;

            Sequence masterSeq = DOTween.Sequence()
                .Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, targetDistortion, attackTime)
                    .SetEase(settings.InitialBounceEase));
                

            for (int i = 1; i <= totalSteps; i++)
            {
                if (i % 2 != 0)
                {
                    masterSeq.Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, 0f, oscillateStepDuration)
                        .SetEase(Ease.InQuad));
                }
                else
                {
                    currentTargetDistortion *= Mathf.Exp(-settings.OscillationFalloff);

                    masterSeq.Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, currentTargetDistortion, oscillateStepDuration)
                        .SetEase(Ease.OutQuad));
                }

                oscillateStepDuration *= Mathf.Exp(-settings.OscillationFalloff);
            }

            masterSeq.SetTarget(_instanceFeedbackTarget)
                .SetUpdate(true)
                .OnComplete(() => OnLenseDistortionPulseComplete(settings, onComplete));
                                    
        }


        private static void ClearLensDistortionPulse()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            if (_lensDistortion) _lensDistortion.intensity.value = 0f;       
        }


        private static void OnLenseDistortionPulseComplete(Settings settings, Action onComplete = null)
        {
            onComplete?.Invoke();
        }


        private static void ValidatePostProcessing()
        {
            var mainCamData = Camera.main.GetUniversalAdditionalCameraData();
            if (!mainCamData.renderPostProcessing) mainCamData.renderPostProcessing = true;
        }

        private static bool TrySpawnLensDistortionPulseSource()
        {
            if (_lensDistortion) return false;

            var newVolumeObj = new GameObject($"LensDistortionPulse_Source");
            var volume = newVolumeObj.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.weight = 1f;
            volume.priority = 100f;

            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _lensDistortion = volume.profile.Add<LensDistortion>(false);
            
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;

            UnityDebug.Log(typeof(LensDistortion), $"LensDistortionPulse source has been successfully added to the scene");
            return true;
        }

 
    }

}
