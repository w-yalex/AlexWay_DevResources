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
        [SerializeField] private Config _defaultConfig;

        private static LensDistortion _lensDistortion;
        private static object _instanceFeedbackTarget = new();

        [Serializable]
        public class Config
        {
            [Header("Intensity")]
            [Range(-1f, 1f)] public float TargetDistortion = 0.7f;

            [Header("Oscillation")]
            public int TotalOscillationLoops = 5;
            [Range(0f, 1f)] public float OscillationFalloff = 0.25f;
    
            [Header("Timing")]
            public float InitialBounceMaxAttackTime = 0.25f;
            public float DecayOscillationTime = 1.5f;
            public bool IgnoreTimeScale;

            [Header("Easing")]
            public Ease InitialBounceEase = Ease.InOutSine;

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
            => BeginLensDistortionPulse(config, onComplete);


        public static void ClearInstance() => ClearLensDistortionPulse();

        private static void BeginLensDistortionPulse(Config config, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            ValidatePostProcessing();
            TrySpawnLensDistortionPulseSource();

            if (config.TargetDistortion == 0) return;

            float targetDistortion = Mathf.Clamp(config.TargetDistortion, -1f, 1f);
            float relativeStart = targetDistortion > 0 ? Mathf.Max(0f, _lensDistortion.intensity.value) : Mathf.Min(0f, _lensDistortion.intensity.value);

            float progressToTarget = Mathf.Clamp01(Mathf.Abs(relativeStart / config.TargetDistortion));
            float attackTime = Mathf.Lerp(config.InitialBounceMaxAttackTime, 0f, progressToTarget);
            
            int totalSteps = Mathf.Max(2, config.TotalOscillationLoops * 2) - 1;
            float oscillateStepDuration = config.DecayOscillationTime / totalSteps;
            float currentTargetDistortion = targetDistortion;

            Sequence masterSeq = DOTween.Sequence()
                .Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, targetDistortion, attackTime)
                    .SetEase(config.InitialBounceEase));
                

            for (int i = 1; i <= totalSteps; i++)
            {
                if (i % 2 != 0)
                {
                    masterSeq.Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, 0f, oscillateStepDuration)
                        .SetEase(Ease.InQuad));
                }
                else
                {
                    currentTargetDistortion *= Mathf.Exp(-config.OscillationFalloff);

                    masterSeq.Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, currentTargetDistortion, oscillateStepDuration)
                        .SetEase(Ease.OutQuad));
                }

                oscillateStepDuration *= Mathf.Exp(-config.OscillationFalloff);
            }

            masterSeq.SetTarget(_instanceFeedbackTarget)
                .SetUpdate(true)
                .OnComplete(() => OnLenseDistortionPulseComplete(config, onComplete));
                                    
        }


        private static void ClearLensDistortionPulse()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            if (_lensDistortion) _lensDistortion.intensity.value = 0f;       
        }


        private static void OnLenseDistortionPulseComplete(Config config, Action onComplete = null)
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
