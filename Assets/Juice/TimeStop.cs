using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class TimeStop : JuiceComponent
    {
        [SerializeField] private OnPlay _onPlayData;

        private static readonly object _instancedTimeStopTarget = new();

        [Serializable]
        public struct OnPlay
        {
            public float PauseDuration;
        }


        public override void PlayOnObject(Action onComplete = null)
            => SetTimeStop(_onPlayData.PauseDuration, this, onComplete);


        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not OnPlay onPlayData)
            {
                UnityDebug.LogWarning(this, "Juice data received did not match OnPlay");
                return;
            }

            SetTimeStop(onPlayData.PauseDuration, this, onComplete);
        }

        public override void StopOnObject()
        {
            DOTween.Kill(this);
        }

        public static void PlayInstanced(OnPlay onPlayData, Action onComplete = null)
            => SetTimeStop(onPlayData.PauseDuration, _instancedTimeStopTarget, onComplete);


        public static void StopInstanced()
        {
            DOTween.Kill(_instancedTimeStopTarget);
        }
        

        private static void SetTimeStop(float pauseDuration, object tweenTarget, Action onComplete = null)
        {
            DOTween.Kill(tweenTarget);

            Time.timeScale = 0f;
            DOVirtual.DelayedCall(pauseDuration, () => Time.timeScale = 1f)
                .SetTarget(tweenTarget)
                .OnComplete(() => onComplete?.Invoke());
        }



    }

}
