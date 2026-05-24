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
        [SerializeField] private Settings _settings;
        private MeshRenderer _meshRenderer;
        
        private MaterialPropertyBlock _propertyBlock;

        [Serializable]
        public struct Settings
        {
            [Header("Visuals")]
            public Color FlashColor;
            
            [Header("Timing")]
            public int FlashCount;
            public float TotalFlashDuration;
            public bool IgnoreTimeScale;
        }


        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();

            _propertyBlock = new MaterialPropertyBlock();
        }


        public override void PlayOnObject(Action onComplete = null)
            => BeginMaterialFlash(_settings, onComplete);
    

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not Settings customSettings)
            {
                UnityDebug.LogWarning(this, $"Juice data received did not match {typeof(Settings).FullName}");
                return;
            }

            BeginMaterialFlash(customSettings, onComplete);
        }

        public override void ClearOnObject() => ClearMaterialFlash();

        private void BeginMaterialFlash(Settings settings, Action onComplete = null)
        {
            DOTween.Kill(this);

            float cycleTime = settings.TotalFlashDuration / settings.FlashCount;
            float flashDelay = cycleTime * 0.5f;

            Sequence seq = DOTween.Sequence()
                .AppendCallback(() => FlashMaterials(settings.FlashColor))
                .AppendInterval(flashDelay)
                .AppendCallback(ResetMaterials)
                .AppendInterval(flashDelay)
                .SetLoops(settings.FlashCount)
                .SetUpdate(settings.IgnoreTimeScale)
                .SetTarget(this)
                .OnComplete(() => OnMaterialFlashComplete(settings, onComplete));
        }

        private void ClearMaterialFlash()
        {
            DOTween.Kill(this);
            ResetMaterials();
        }

        private void OnMaterialFlashComplete(Settings settings, Action onComplete = null)
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
