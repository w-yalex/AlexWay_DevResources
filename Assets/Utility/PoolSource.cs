using UnityEngine;
using UnityEngine.Pool;
using System;


namespace AW.UnityResources
{
    [Serializable]
    public class PoolSource<TComponent> where TComponent : Component
    {
 
        [SerializeField] private TComponent _pooledPrefab;
        [SerializeField] private int _defaultCapacity;
        [SerializeField] private int _maxSize;

        public event Action<TComponent> OnComponentGet;
        public event Action<TComponent> OnComponentRelease;
    
        private ObjectPool<TComponent> _collection;


        public TComponent PooledPrefab => _pooledPrefab;
        public int DefaultCapacity => _defaultCapacity;
        public int MaxSize => _maxSize;

        public ObjectPool<TComponent> Collection
        {
            get
            {
                if (_collection == null)
                    InitPool();


                return _collection;
            }
        }


        public void InitPool() // Call this in the Awake of implemntation script to prevent lag spike on first Get()
        {
            _collection?.Clear();
            _collection = new ObjectPool<TComponent>
            (
                CreateComponentObject,
                OnGet,
                OnRelease,
                DestroyComponentObject,
                true,
                _defaultCapacity,
                _maxSize
            );
        }

        public virtual void OverrideCollection(TComponent newPooledPrefab, int defaultCapacity, int maxSize)
        {
            _pooledPrefab = newPooledPrefab;
            _defaultCapacity = defaultCapacity;
            _maxSize = maxSize;

            InitPool();
        }

        protected virtual TComponent CreateComponentObject()
        {
            TComponent componentObj = UnityEngine.Object.Instantiate(_pooledPrefab);
            componentObj.gameObject.SetActive(false);

            return componentObj;
        }

        protected virtual void OnGet(TComponent component)
        {
            component.gameObject.SetActive(true);
            OnComponentGet?.Invoke(component);
        }

        protected virtual void OnRelease(TComponent component)
        {
            if (!component.gameObject.activeSelf) return;

            component.gameObject.SetActive(false);
            OnComponentRelease?.Invoke(component);
        }

        protected virtual void DestroyComponentObject(TComponent component)
            => UnityEngine.Object.Destroy(component.gameObject);
    }

}