using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
  
    public class ScreenFlashStrobe : JuiceComponent
    {
        [Header("Screen Flash Strobe")]
        [SerializeField] private Config _defaultConfig;

        private static ColorAdjustments _colorAdjustments;
        private static readonly object _instanceFeedbackTarget = new();

        [Serializable]
        public class Config
        {
            [Header("Visuals")]
            public Color FlashColor = Color.white;

            [Tooltip("Values 1 to 3 usually feel better")]
            public float FlashExposure = 1.2f;

            [Header("Timing")]
            public int FlashCycles = 4;
            public float TotalFlashDuration = 0.2f;
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
            => BeginSceenFlashStrobe(config, onComplete);


        public static void ClearInstance() => ClearScreenBlinkFlash();

        private static void BeginSceenFlashStrobe(Config config, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            ValidatePostProcessing();
            TrySpawnScreenFlashStrobeSource();

            ApplyScreenBlinkFlashConfig(config);

            float cycleTime = config.TotalFlashDuration / config.FlashCycles;
            float flashDelay = cycleTime * 0.5f;

            Sequence seq = DOTween.Sequence()
                .AppendCallback(() => _colorAdjustments.postExposure.value = config.FlashExposure)
                .AppendInterval(flashDelay)
                .AppendCallback(() => _colorAdjustments.postExposure.value = 0f)
                .AppendInterval(flashDelay)
                .SetLoops(config.FlashCycles)
                .SetUpdate(config.IgnoreTimeScale)
                .SetTarget(_instanceFeedbackTarget)
                .OnComplete(() => OnScreenFlashStrobe(config, onComplete));
            
        }

        
        private static void ClearScreenBlinkFlash() // TODO: Add OnStop data
        {
            DOTween.Kill(_instanceFeedbackTarget);
            if (_colorAdjustments) _colorAdjustments.postExposure.value = 0f;         
        }


        private static void OnScreenFlashStrobe(Config config, Action onComplete = null)
        {
            _colorAdjustments.colorFilter.value = Color.white;
            onComplete?.Invoke();
        }

        private static void ValidatePostProcessing()
        {
            var mainCamData = Camera.main.GetUniversalAdditionalCameraData();
            if (!mainCamData.renderPostProcessing) mainCamData.renderPostProcessing = true;
        }

        private static bool TrySpawnScreenFlashStrobeSource()
        {
            if (_colorAdjustments) return false;

            var newVolumeObj = new GameObject($"ScreenFlashStrobe_Source");
            var volume = newVolumeObj.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.weight = 1f;
            volume.priority = 100f;

            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            _colorAdjustments = volume.profile.Add<ColorAdjustments>(false);
            
            _colorAdjustments.colorFilter.overrideState = true;
            _colorAdjustments.postExposure.overrideState = true;
            _colorAdjustments.postExposure.value = 0f;

            UnityDebug.Log(typeof(ScreenFlashStrobe), $"ScreenFlashStrobe source has been successfully added to the scene");
            return true;
        }

        private static void ApplyScreenBlinkFlashConfig(Config config)
        {
            _colorAdjustments.colorFilter.value = config.FlashColor;
            // TODO: Maybe modify intensity 
        }


    }

}
