using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeScaleSlowMotion : JuiceComponent
    {
        public const float MaxSlowMotionFactor = 0.99f;
        [Header("Time Scale Slow Motion")]
        [SerializeField] private Config _config;

        [Serializable]
        public class Config
        {
            [Header("Timing")]
            [Tooltip("The higher value, the greater the slow motion")]
            [Range(0f, MaxSlowMotionFactor)] public float TargetSlowMotionFactor = 0.8f;
            public float MaxAttackTime = 0.2f;
            public float SustainTime = 0.2f;
            public float DecayTime = 0.5f;

            [Header("Easing")]
            public Ease AttackEase = Ease.InOutSine;
            public Ease DecayEase = Ease.InQuad;

            [Header("Override")]
            public int OverridePriority = -1;
        }

        public override void PlayOnObject(Action onComplete = null)
            => PlayInstance(_config, onComplete);

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
            => BeginTimeScaleSlowMotion(config, onComplete);

        public static void ClearInstance() => ClearTimeScaleSlowMotion();
        
        private static void BeginTimeScaleSlowMotion(Config config, Action onComplete = null)
        {
            if (!TimeScale.TrySetActiveModifier(TimeScale.Modifier.SlowMotion, config.OverridePriority)) return;
            DOTween.Kill(TimeScale.ModifierTarget);

            float targetSlowMotionFactor = Mathf.Clamp(config.TargetSlowMotionFactor, 0f, MaxSlowMotionFactor);

            float startTimeScale = TimeScale.GetCurrent();
            float targetTimeScale = 1 - config.TargetSlowMotionFactor;
            float progressToTarget = Mathf.InverseLerp(1f, targetTimeScale, startTimeScale);
            float attackTime = (1 - progressToTarget) * config.MaxAttackTime;

            float t = 0f;
            DOTween.Sequence()
                .Append(DOTween.To(() => t, x => t = x, 1f, attackTime)
                    .OnUpdate(() =>
                    {
                        float appliedTimeScale = Mathf.Lerp(startTimeScale, targetTimeScale, t);
                        TimeScale.Override(appliedTimeScale);
                    })
                    .SetEase(config.AttackEase)
                    .OnComplete(() => t = 0f))
                .AppendInterval(config.SustainTime)
                .Append(DOTween.To(() => t, x => t = x, 1f, config.DecayTime)
                    .OnUpdate(() =>
                    {
                        float appliedTimeScale = Mathf.Lerp(targetTimeScale, 1f, t);
                        TimeScale.Override(appliedTimeScale);
                    })
                    .SetEase(config.DecayEase))
                .SetUpdate(true)
                .SetTarget(TimeScale.ModifierTarget)
                .OnComplete(() =>
                {
                    TimeScale.TryClearActiveModifier(TimeScale.Modifier.SlowMotion);
                    onComplete?.Invoke();
                });
        }

        private static void ClearTimeScaleSlowMotion()
        {
            if (!TimeScale.TryClearActiveModifier(TimeScale.Modifier.SlowMotion)) return;
            
            DOTween.Kill(TimeScale.ModifierTarget);
            TimeScale.Override(1f);
        }


    }

}
