using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeScaleFreeze : JuiceComponent
    {
        [Header("Time Scale Freeze")]
        [SerializeField] private Config _defaultConfig;

        [Serializable]
        public class Config
        {
            [Header("Timing")]
            public float FreezeDuration = 0.1f;

            [Header("Override")]
            public int OverridePriority = -1;
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
            => BeginTimeScaleFreeze(config, onComplete);

        public static void ClearInstance() => ClearTimeScaleFreeze();
        
        private static void BeginTimeScaleFreeze(Config config, Action onComplete = null)
        {
            if (!TimeScale.TrySetActiveModifier(TimeScale.Modifier.Freeze, config.OverridePriority)) return;
            DOTween.Kill(TimeScale.ModifierTarget);

            TimeScale.Override(0f);
            DOVirtual.DelayedCall(config.FreezeDuration, () =>
                {
                    TimeScale.Override(1f);
                    TimeScale.TryClearActiveModifier(TimeScale.Modifier.Freeze);
                    
                    onComplete?.Invoke();
                })
                .SetUpdate(true)
                .SetTarget(TimeScale.ModifierTarget);
        }

        private static void ClearTimeScaleFreeze()
        {
            if (!TimeScale.TryClearActiveModifier(TimeScale.Modifier.Freeze)) return;
            
            DOTween.Kill(TimeScale.ModifierTarget);
            TimeScale.Override(1f);
            
        }






    }

}
