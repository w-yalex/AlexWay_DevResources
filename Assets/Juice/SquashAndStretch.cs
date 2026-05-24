using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class SquashAndStretch : JuiceComponent
    {
        [Header("Squash And Stretch")]
        [SerializeField] private Settings _settings;
        private Vector3 _objOriginalScale;

        [Serializable]
        public struct Settings
        {
            [Header("Direction")]

            public bool IsDirectional;
            public Vector3 OptionalPressureDirection;

            [Header("Shape")]
            [Range(0f, 1f)] public float SquashAmount;
            [Range(0f, 1f)] public float StretchAmount;

            [Header("Timing")]
            public float SquashTime;
            public float StretchTime;
            public float OscillateDecayTime;
            public bool IgnoreTimeScale;
        }

        private void Awake()
        {
            _objOriginalScale = transform.localScale;
        }

        public override void PlayOnObject(Action onComplete = null)
            => BeginSquashAndStretch(_settings, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Settings customSettings)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Settings).FullName}");
                return;
            }

            BeginSquashAndStretch(customSettings, onComplete);
        }

        public override void ClearOnObject() => ClearSquashAndStretch();

        private void BeginSquashAndStretch(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(this);

            Vector3 squashScale = _objOriginalScale * _settings.SquashAmount;
            Vector3 stretchScale = _objOriginalScale * (1 + _settings.StretchAmount);

            if (settings.IsDirectional)
            {
                Vector3 pressureDir = settings.OptionalPressureDirection.normalized;
                Vector3 weightedAxisPressure = new Vector3(Mathf.Abs(pressureDir.x), Mathf.Abs(pressureDir.y), Mathf.Abs(pressureDir.z));

                float squashFactor = Mathf.Max(0.001f, 1f - settings.SquashAmount);
                float stretchFactor = 1f + Mathf.Clamp01(settings.StretchAmount);

                squashScale = GetDirectionalTargetScale(squashFactor, weightedAxisPressure);
                stretchScale = GetDirectionalTargetScale(stretchFactor, weightedAxisPressure);   
            }

    
            DOTween.Sequence()
                .Append(transform.DOScale(squashScale, settings.SquashTime)
                    .SetEase(Ease.InOutSine))
                .Append(transform.DOScale(stretchScale, settings.StretchTime)
                    .SetEase(Ease.InQuad))
                .Append(transform.DOScale(_objOriginalScale, settings.OscillateDecayTime)
                    .SetEase(Ease.OutElastic))
                .SetUpdate(settings.IgnoreTimeScale)
                .SetTarget(this)
                .OnComplete(() => OnSquashAndStretchComplete(settings, onComplete));
        }


        private void ClearSquashAndStretch()
        {
            DOTween.Kill(this);
            transform.localScale = _objOriginalScale;
        }

        private void OnSquashAndStretchComplete(Settings settings, Action onComplete = null)
        {
            onComplete?.Invoke();
        }


        private Vector3 GetDirectionalTargetScale(float scaleFactor, Vector3 weightedAxisPressure)
        {
            float compensationFactor = 1f / Mathf.Sqrt(scaleFactor);

            Vector3 adjustedScale = new Vector3(
                Mathf.Lerp(compensationFactor, scaleFactor, weightedAxisPressure.x),
                Mathf.Lerp(compensationFactor, scaleFactor, weightedAxisPressure.y),
                Mathf.Lerp(compensationFactor, scaleFactor, weightedAxisPressure.z)
            );

            return Vector3.Scale(_objOriginalScale, adjustedScale);

        }



    }
}