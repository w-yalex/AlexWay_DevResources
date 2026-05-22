using UnityEngine;
using DG.Tweening;
using System;

namespace AW.UnityResources
{
    // These are placed on GameObjects, some will have methods to call static Play()
    public abstract class JuiceComponent : MonoBehaviour
    {
        public virtual void PlayOnObject(Action onComplete = null) { }

        // Override the default values with custom values
        public virtual void PlayOnObject<TData>(TData juiceData, Action onComplete = null) where TData : struct { }


        public virtual void StopOnObject() { }

        public virtual void StopOnObject<TData>(TData juiceData) where TData : struct { }

        protected virtual void OnDestroy() => DOTween.Kill(this);
        
    }

}
