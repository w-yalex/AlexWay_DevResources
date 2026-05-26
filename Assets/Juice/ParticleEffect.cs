using UnityEngine;
using System;
using DG.Tweening;
using System.Collections;


namespace AW.UnityResources
{
    public class ParticleEffect : JuiceComponent
    {  
        [Header("Particle Effect")]
        [SerializeField] private Config _config;

        [Serializable]
        public class Config
        {
            public PoolSource<ParticleSystem> ParticlePoolSource;
            public Transform[] SpawnLocations = Array.Empty<Transform>();

            public float CustomPlayDuration = -1f;
        }
        

        private void Awake()
        {
            if (_config.ParticlePoolSource == null)
            {
                UnityDebug.LogError(this, $"No PartilcePoolSource configured on {gameObject}");
                return;
            }

            _config.ParticlePoolSource.InitPool();
        }


        public override void PlayOnObject(Action onComplete = null)
            => PlayParticleEffect(_config, onComplete);
    

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Config customConfig)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Config).FullName}");
                return;
            }

            PlayParticleEffect(customConfig, onComplete);
        }

        public override void ClearOnObject() => ClearParticleEffect();

        private void PlayParticleEffect(Config config, Action onComplete = null)
        {
           
            foreach (Transform spawn in config.SpawnLocations)
                SpawnParticle(spawn);

            void SpawnParticle(Transform spawn)
            {
                var particleSystem = config.ParticlePoolSource.Collection.Get();

                particleSystem.transform.SetParent(spawn, false);
                particleSystem.transform.localPosition = Vector3.zero;
                particleSystem.transform.localRotation = Quaternion.identity;

                particleSystem.transform.SetAsLastSibling();
                particleSystem.Play();

                if (config.CustomPlayDuration != 1)
                {
                    DOVirtual.DelayedCall(config.CustomPlayDuration, () =>
                    {
                        if (particleSystem != null && particleSystem.gameObject.activeSelf)
                        {
                            config.ParticlePoolSource.Collection.Release(particleSystem);
                            onComplete?.Invoke();
                        }
                    })
                    .SetTarget(this);
                }
                else StartCoroutine(WaitUntilFinish());
            }
        
            IEnumerator WaitUntilFinish()
            {
                yield return new WaitUntil(() => GetComponent<ParticleSystem>() == null || !GetComponent<ParticleSystem>().IsAlive());

                if (GetComponent<ParticleSystem>() != null && GetComponent<ParticleSystem>().gameObject.activeSelf)
                {
                    config.ParticlePoolSource.Collection.Release(GetComponent<ParticleSystem>());
                    onComplete?.Invoke();
                }
            }
        }


        private void ClearParticleEffect()
        {
            DOTween.Kill(this);
            _config.ParticlePoolSource.Collection.Clear();

        }


    }

}
