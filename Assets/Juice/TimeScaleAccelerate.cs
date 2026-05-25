using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeScaleAccelerate : JuiceComponent
    {
        public const float MaxAccelerateFactor = 10f;

        [Header("Time Scale Accelerate")]
        [SerializeField] private Config _config;

        [Serializable]
        public class Config
        {
            [Header("Timing")]

            [Tooltip("The higher value, the greater the slow motion")]
            [Range(1f, MaxAccelerateFactor)] public float targetTimeScale = 0.8f;
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
            => BeginTimeScaleAccelerate(config, onComplete);

        public static void ClearInstance() => ClearTimeScaleSlowMotion();
        
        private static void BeginTimeScaleAccelerate(Config config, Action onComplete = null)
        {
            if (!TimeScale.TrySetActiveModifier(TimeScale.Modifier.Accelerate, config.OverridePriority)) return;
            DOTween.Kill(TimeScale.ModifierTarget);

            float startingTimeScale = TimeScale.GetCurrent();

            float targetTimeScale = Mathf.Clamp(config.targetTimeScale, 1f, MaxAccelerateFactor);
            float progressToTarget = Mathf.InverseLerp(1f, targetTimeScale, startingTimeScale);

            float attackTime = (1 - progressToTarget) * config.MaxAttackTime;

            float t = 0f;
            DOTween.Sequence()
                .Append(DOTween.To(() => 0f, x => t = x, 1f, attackTime)
                    .OnUpdate(() =>
                    {
                        float appliedTimeScale = Mathf.Lerp(startingTimeScale, targetTimeScale, t);
                        TimeScale.Override(appliedTimeScale);
                    })
                    .SetEase(config.AttackEase)
                    .OnComplete(() => t = 0f))
                .AppendInterval(config.SustainTime)
                .Append(DOTween.To(() => 0f, x => t = x, 1f, config.DecayTime)
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
                    TimeScale.TryClearActiveModifier(TimeScale.Modifier.Accelerate);
                    onComplete?.Invoke();
                });
        }


        private static void ClearTimeScaleSlowMotion()
        {
            if (!TimeScale.TryClearActiveModifier(TimeScale.Modifier.Accelerate)) return;

            DOTween.Kill(TimeScale.ModifierTarget);
            Time.timeScale = 1f;
        }



    }

}
