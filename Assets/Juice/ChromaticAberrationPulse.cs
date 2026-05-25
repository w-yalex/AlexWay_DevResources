using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AW.UnityResources
{
    public class ChromaticAberrationPulse : JuiceComponent // Pulse once time 
    {
  
        [Header("Chromatic Aberration Pulse")]
        [SerializeField] private Config _defaultConfig;

        private static ChromaticAberration _chromaticAbberation;
        private static readonly object _instanceFeedbackTarget = new();

        [Serializable]
        public class Config
        {
            [Header("Intensity")]
            [Range(0f, 1f)] public float MaxIntensity = 1f;

            [Header("Timing")]

            public float MaxAttackTime = 0.1f;
            public float SustainTime = 0.2f;
            public float DecayTime = 0.5f;
            public bool IgnoreTimeScale;
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
            => BeginChromaticAberrationPulse(config, onComplete);


        public static void ClearInstance() => ClearChromaticAberrationPulse();

        private static void BeginChromaticAberrationPulse(Config config, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            ValidatePostProcessing();
            TrySpawnChromaticAberrationPulseSource();

            float progressToTarget = Mathf.Clamp01(_chromaticAbberation.intensity.value / config.MaxIntensity);
            float attackTime = Mathf.Lerp(config.MaxIntensity, 0f, progressToTarget);

            Sequence seq = DOTween.Sequence()
                .Append(DOTween.To(() => _chromaticAbberation.intensity.value, x => _chromaticAbberation.intensity.value = x, config.MaxIntensity, attackTime)
                    .SetEase(Ease.InOutSine))
                .AppendInterval(config.SustainTime)
                .Append(DOTween.To(() => _chromaticAbberation.intensity.value, x => _chromaticAbberation.intensity.value = x, 0f, config.DecayTime)
                    .SetEase(Ease.InOutSine))
                .SetUpdate(config.IgnoreTimeScale)
                .SetTarget(_instanceFeedbackTarget)
                .OnComplete(() => OnChromaticAberrationPulseComplete(config, onComplete));
        }

        
        private static void ClearChromaticAberrationPulse() 
        {
            DOTween.Kill(_instanceFeedbackTarget);
            if (_chromaticAbberation) _chromaticAbberation.intensity.value = 0f;       
        }


        private static void OnChromaticAberrationPulseComplete(Config config, Action onComplete = null)
        {
            onComplete?.Invoke();
        }


        private static void ValidatePostProcessing()
        {
            var mainCamData = Camera.main.GetUniversalAdditionalCameraData();
            if (!mainCamData.renderPostProcessing) mainCamData.renderPostProcessing = true;
        }

        private static bool TrySpawnChromaticAberrationPulseSource()
        {
            if (_chromaticAbberation) return false;

            var newVolumeObj = new GameObject($"ChromaticAberrationPulse_Source");
            var volume = newVolumeObj.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.weight = 1f;
            volume.priority = 100f;

            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _chromaticAbberation = volume.profile.Add<ChromaticAberration>(false);
            
            _chromaticAbberation.intensity.overrideState = true;
            _chromaticAbberation.intensity.value = 0f;

            UnityDebug.Log(typeof(ChromaticAberrationPulse), $"Chromatic Aberration Pulse source has been successfully added to the scene");
            return true;
        }

    }

}
