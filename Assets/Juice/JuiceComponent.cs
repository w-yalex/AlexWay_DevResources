using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    /// <summary>
    /// These are placed on GameObjects and called via Juice static class
    /// Some class may have a PlayInstanced() if they don't need to exist on any single GameObject
    /// </summary>
    public abstract class JuiceComponent : MonoBehaviour
    {
        [Header("Juice Component")]

        [Tooltip("Can be used to reference a specific JuiceComponent")]
        [SerializeField] private string _optionalReferenceKey;
        
        public string OptionalReferenceKey => _optionalReferenceKey;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_optionalReferenceKey))
                _optionalReferenceKey = null;
        }

        
        public abstract void PlayOnObject(Action onComplete = null);

        public abstract void PlayOnObject<TData>(TData juiceData, Action onComplete = null) where TData : class;

        public abstract void ClearOnObject();
        
    }

}
