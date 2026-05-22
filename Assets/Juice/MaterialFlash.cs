using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;


namespace AW.UnityResources
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MaterialFlash : JuiceComponent
    {  
        [SerializeField] private OnPlay _onPlayData;

        private Dictionary<int, Material> _originalMaterialsByIndex = new();
        private Material[] _currentMaterials;
        private MeshRenderer _meshRenderer;

        [Serializable]
        public struct OnPlay
        {
            public Material FlashMaterial;
            public int FlashCount;
            public float TotalFlashDuration;
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();

            _currentMaterials = _meshRenderer.materials;

            for (int i = 0; i < _currentMaterials.Length; i++)
                _originalMaterialsByIndex[i] = _currentMaterials[i];

        }


        public override void PlayOnObject(Action onComplete = null)
            => SetMaterialFlash(_onPlayData, onComplete);
    

        public override void PlayOnObject<TData>(TData juiceData, Action onComplete = null)
        {
            if (juiceData is not OnPlay onPlayData)
            {
                UnityDebug.LogWarning(this, "Juice data received did not match OnPlay");
                return;
            }

            SetMaterialFlash(onPlayData, onComplete);
        }


        private void SetMaterialFlash(OnPlay onPlayData, Action onComplete = null)
        {
            DOTween.Kill(this);

            float cycleTime = onPlayData.TotalFlashDuration / onPlayData.FlashCount;
            float flashDelay = cycleTime * 0.5f;

            Sequence seq = DOTween.Sequence()
                .AppendCallback(() => FlashMaterials(onPlayData.FlashMaterial))
                .AppendInterval(flashDelay)
                .AppendCallback(ResetMaterials)
                .AppendInterval(flashDelay)
                .SetLoops(onPlayData.FlashCount)
                .SetTarget(this)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void FlashMaterials(Material flashMaterial)
        {
            for (int i = 0; i < _meshRenderer.materials.Length; i++)
                _currentMaterials[i] = flashMaterial;
                
            _meshRenderer.materials  = _currentMaterials;
        }

        private void ResetMaterials()
        {
            for (int i = 0; i < _meshRenderer.materials.Length; i++)
                _currentMaterials[i] = _originalMaterialsByIndex[i];
    
            _meshRenderer.materials = _currentMaterials;
        }

        public override void StopOnObject()
        {
            DOTween.Kill(this);
            ResetMaterials();
        }

    }

}
