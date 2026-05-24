using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeScaleFreeze : JuiceComponent
    {
        [Header("Time Scale Freeze")]
        [SerializeField] private Settings _settings;

        private static readonly object _instanceFeedbackTarget = new();

        [Serializable]
        public struct Settings
        {
            public float FreezeDuration;
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
            => BeginTimeScaleFreeze(settings, onComplete);

        public static void ClearInstance() => ClearTimeScaleFreeze();
        
        private static void BeginTimeScaleFreeze(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            Time.timeScale = 0f;

            float elapsedTime = 0f;
            DOTween.To(() => elapsedTime, x => elapsedTime = x, settings.FreezeDuration, settings.FreezeDuration)
                .OnUpdate(() => Time.timeScale = 0f)
                .SetUpdate(UpdateType.Late, true) // Applies after any TimeScaleSlowMotion to override it 
                .SetTarget(_instanceFeedbackTarget);
        
        }

        private static void ClearTimeScaleFreeze()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            Time.timeScale = 1f;
        }


        private static void OnTimeScaleFreezeComplete(Settings settings, Action onComplete = null)
        {
            Time.timeScale = 1f;
            onComplete?.Invoke();
        }



    }

}
