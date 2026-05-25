using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    public class SquashAndStretch : JuiceComponent
    {
        [Header("Squash And Stretch")]
        [SerializeField] private Config _defaultConfig;
        private Vector3 _objOriginalScale;

        [Serializable]
        public class Config
        {
            [Header("Direction")]

            public bool IsDirectional;
            public Vector3 OptionalPressureDirection;

            [Header("Shape")]
            [Range(0f, 1f)] public float SquashAmount = 0.8f;
            [Range(0f, 1f)] public float StretchAmount = 0.9f;

            [Header("Timing")]
            public float SquashTime = 0.4f;
            public float StretchTime = 0.2f;
            public float OscillateDecayTime = 1.5f;
            public bool IgnoreTimeScale;
        }

        private void Awake()
        {
            _objOriginalScale = transform.localScale;
        }

        public override void PlayOnObject(Action onComplete = null)
            => BeginSquashAndStretch(_defaultConfig, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Config customConfig)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Config).FullName}");
                return;
            }

            BeginSquashAndStretch(customConfig, onComplete);
        }

        public override void ClearOnObject() => ClearSquashAndStretch();

        private void BeginSquashAndStretch(Config config, Action onComplete = null)
        {
            DOTween.Kill(this);

            Vector3 squashScale = _objOriginalScale * _defaultConfig.SquashAmount;
            Vector3 stretchScale = _objOriginalScale * (1 + _defaultConfig.StretchAmount);

            if (config.IsDirectional)
            {
                Vector3 pressureDir = transform.InverseTransformDirection(config.OptionalPressureDirection).normalized;
                Vector3 weightedAxisPressure = new Vector3(Mathf.Abs(pressureDir.x), Mathf.Abs(pressureDir.y), Mathf.Abs(pressureDir.z));

                float squashFactor = Mathf.Max(0.001f, 1f - config.SquashAmount);
                float stretchFactor = 1f + Mathf.Clamp01(config.StretchAmount);

                squashScale = GetDirectionalTargetScale(squashFactor, weightedAxisPressure);
                stretchScale = GetDirectionalTargetScale(stretchFactor, weightedAxisPressure);   
            }

    
            DOTween.Sequence()
                .Append(transform.DOScale(squashScale, config.SquashTime)
                    .SetEase(Ease.InOutSine))
                .Append(transform.DOScale(stretchScale, config.StretchTime)
                    .SetEase(Ease.InQuad))
                .Append(transform.DOScale(_objOriginalScale, config.OscillateDecayTime)
                    .SetEase(Ease.OutElastic))
                .SetUpdate(config.IgnoreTimeScale)
                .SetTarget(this)
                .OnComplete(() => OnSquashAndStretchComplete(config, onComplete));
        }


        private void ClearSquashAndStretch()
        {
            DOTween.Kill(this);
            transform.localScale = _objOriginalScale;
        }

        private void OnSquashAndStretchComplete(Config config, Action onComplete = null)
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