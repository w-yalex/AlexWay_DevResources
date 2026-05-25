using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;


namespace AW.UnityResources
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialFlash : JuiceComponent
    {  
        [Header("Material Flash")]
        [SerializeField] private Config _defaultConfig;
        private MeshRenderer _meshRenderer;
        
        private MaterialPropertyBlock _propertyBlock;

        [Serializable]
        public class Config
        {
            [Header("Visuals")]
            public Color FlashColor = Color.white;
            
            [Header("Timing")]
            public int FlashCycles = 4;
            public float TotalFlashDuration = 0.2f;
            public bool IgnoreTimeScale;
        }


        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }


        public override void PlayOnObject(Action onComplete = null)
            => BeginMaterialFlash(_defaultConfig, onComplete);
    

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Config customConfig)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Config).FullName}");
                return;
            }

            BeginMaterialFlash(customConfig, onComplete);
        }

        public override void ClearOnObject() => ClearMaterialFlash();

        private void BeginMaterialFlash(Config config, Action onComplete = null)
        {
            DOTween.Kill(this);

            float cycleTime = config.TotalFlashDuration / config.FlashCycles;
            float flashDelay = cycleTime * 0.5f;

            Sequence seq = DOTween.Sequence()
                .AppendCallback(() => FlashMaterials(config.FlashColor))
                .AppendInterval(flashDelay)
                .AppendCallback(ResetMaterials)
                .AppendInterval(flashDelay)
                .SetLoops(config.FlashCycles)
                .SetUpdate(config.IgnoreTimeScale)
                .SetTarget(this)
                .OnComplete(() => OnMaterialFlashComplete(config, onComplete));
        }

        private void ClearMaterialFlash()
        {
            DOTween.Kill(this);
            ResetMaterials();
        }

        private void OnMaterialFlashComplete(Config config, Action onComplete = null)
        {
            ClearMaterialFlash();
            onComplete?.Invoke();
        }

        private void FlashMaterials(Color flashColor)
        {
           _meshRenderer.GetPropertyBlock(_propertyBlock);

           _propertyBlock.SetColor("_BaseColor", flashColor);
           _meshRenderer.SetPropertyBlock(_propertyBlock);
        }


        private void ResetMaterials()
        {
            _propertyBlock.Clear();
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }


    }

}
