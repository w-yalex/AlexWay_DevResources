using UnityEngine;
using UnityEngine.Pool;

namespace AW.UnityResources
{
    public class PoolHandle<TComponent> where TComponent : Component
    {
        public TComponent ActiveComponent { get; private set; }
        public PoolSource<TComponent> PoolSource { get; private set; }
        
        public PoolHandle(TComponent activeComponent, PoolSource<TComponent> poolSource)
        {
            ActiveComponent = activeComponent;
            PoolSource = poolSource;
        }
        

        public bool TryRelease()
        {
            if (!ActiveComponent.gameObject.activeSelf || PoolSource == null) return false;

            PoolSource.Collection.Release(ActiveComponent);
            return true;
        }

    }
}