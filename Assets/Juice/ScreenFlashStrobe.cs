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
        [SerializeField] private Settings _settings;

        private static ColorAdjustments _colorAdjustments;
        private static readonly object _instanceFeedbackTarget = new();

        [Serializable]
        public struct Settings
        {
            [Header("Visuals")]
            public Color FlashColor;
            public float FlashExposure;

            [Header("Timing")]
            public int FlashCount;
            public float TotalFlashDuration;
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
            => BeginSceenFlashStrobe(settings, onComplete);


        public static void ClearInstance() => ClearScreenBlinkFlash();

        private static void BeginSceenFlashStrobe(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            ValidatePostProcessing();
            TrySpawnScreenFlashStrobeSource();

            ApplyScreenBlinkFlashSettings(settings);

            float cycleTime = settings.TotalFlashDuration / settings.FlashCount;
            float flashDelay = cycleTime * 0.5f;

            Sequence seq = DOTween.Sequence()
                .AppendCallback(() => _colorAdjustments.postExposure.value = settings.FlashExposure)
                .AppendInterval(flashDelay)
                .AppendCallback(() => _colorAdjustments.postExposure.value = 0f)
                .AppendInterval(flashDelay)
                .SetLoops(settings.FlashCount)
                .SetUpdate(settings.IgnoreTimeScale)
                .SetTarget(_instanceFeedbackTarget)
                .OnComplete(() => OnScreenFlashStrobe(settings, onComplete));
            
        }

        
        private static void ClearScreenBlinkFlash() // TODO: Add OnStop data
        {
            DOTween.Kill(_instanceFeedbackTarget);
            if (_colorAdjustments) _colorAdjustments.postExposure.value = 0f;         
        }


        private static void OnScreenFlashStrobe(Settings settings, Action onComplete = null)
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

        private static void ApplyScreenBlinkFlashSettings(Settings settings)
        {
            _colorAdjustments.colorFilter.value = settings.FlashColor;
            // TODO: Maybe modify intensity 
        }


    }

}
