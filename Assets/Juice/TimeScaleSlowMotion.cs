using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeScaleSlowMotion : JuiceComponent
    {
        [Header("Time Scale Slow Motion")]
        [SerializeField] private Settings _settings;

        private static readonly object _instanceFeedbackTarget = new();

        [Serializable]
        public struct Settings
        {
            [Header("Timing")]

            [Range(0f, 1f)] public float TargetTimeScale;
            public float MaxAttackTime;
            public float SustainTime;
            public float DecayTime;

            [Header("Easing")]
            public Ease AttackEase;
            public Ease DecayEase;
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
            => BeginTimeScaleSlowMotion(settings, onComplete);

        public static void ClearInstance() => ClearTimeScaleSlowMotion();
        
        private static void BeginTimeScaleSlowMotion(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(_instanceFeedbackTarget);

            float targetTimeScale = Mathf.Clamp01(settings.TargetTimeScale);
            float progressToTarget = Mathf.InverseLerp(1f, targetTimeScale, Time.timeScale);

            float attackTime = (1 - progressToTarget) * settings.MaxAttackTime;

            DOTween.Sequence()
                .Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, targetTimeScale, attackTime)
                    .SetEase(settings.AttackEase))
                .AppendInterval(settings.SustainTime)
                .Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, settings.DecayTime)
                    .SetEase(settings.DecayEase))
                .SetUpdate(true)
                .SetTarget(_instanceFeedbackTarget)
                .OnComplete(() => OnTimeScaleSlowComplete(settings, onComplete));
        }

        private static void ClearTimeScaleSlowMotion()
        {
            DOTween.Kill(_instanceFeedbackTarget);
            Time.timeScale = 1f;
        }


        private static void OnTimeScaleSlowComplete(Settings settings, Action onComplete = null)
        {
            onComplete?.Invoke();
        }



    }

}
