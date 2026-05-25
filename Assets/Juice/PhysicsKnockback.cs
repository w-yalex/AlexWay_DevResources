using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsKnockback : JuiceComponent
    {
        [Header("Physics Knockback")]
        [SerializeField] private Config _defaultConfig;

        [Serializable]
        public class Config
        {
            [Header("Physics")]

            public Vector3 ForceDirection = new Vector3(0f, 0f, 1f);
            public float ForceMagnitude = 10f;
            public ForceMode ForceMode = ForceMode.Impulse;
      
        }

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public override void PlayOnObject(Action onComplete = null)
            => BeginKnockback(_defaultConfig, onComplete);

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Config customConfig)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Config).FullName}");
                return;
            }

            BeginKnockback(customConfig, onComplete);
        }

        public override void ClearOnObject() { }

        private void BeginKnockback(Config config, Action onComplete = null)
        {
            DOTween.Kill(this);
            _rb.AddForce(config.ForceDirection.normalized * config.ForceMagnitude, config.ForceMode);

            OnKnockbackComplete(config, onComplete);
        }


        private void OnKnockbackComplete(Config config, Action onComplete = null)
        {
            onComplete?.Invoke();
        }





    }
}