using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Events;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Placed on the root GameObject of a prefab to give it automatic object pooling.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        private static Dictionary<int, Pool> pools = new Dictionary<int, Pool>();

        private class Pool
        {
            public PooledObject prefab;
            Queue<PooledObject> pool = new Queue<PooledObject>();

            public Pool(PooledObject prefab)
            {
                this.prefab = prefab;
            }

            public PooledObject Get()
            {
                PooledObject obj = null;
                if (pool.Count > 0)
                {
                    obj = pool.Dequeue();
                    if (obj == null) obj = Get();
                    obj.gameObject.SetActive(true);
                }
                if (pool.Count == 0 && obj == null)
                {
                    obj = Instantiate(prefab);
                }
                //obj = Instantiate(prefab);
                return obj;
            }

            public void Release(PooledObject obj)
            {
                pool.Enqueue(obj);
                //Debug.Log($"Pool size: {pool.Count} - {currentSize}/{maxSize}");
                //Destroy(obj);
                //currentSize--;
            }
        }

        [SerializeField, HideInInspector]
        private int poolID;

        public UnityEvent<PooledObject> OnGet;
        public System.Action<PooledObject> OnGetReset;
        public UnityEvent<PooledObject> OnRelease;
        public System.Action<PooledObject> OnReleaseReset;

        public T RequestObject<T>() where T : PooledObject
        {
            CheckForPool();
            PooledObject obj = pools[poolID].Get();
            obj.GetComponent<PooledObject>().ResetObject();
            OnGet?.Invoke(obj);
            OnGetReset?.Invoke(obj);
            OnReleaseReset = null;
            OnGetReset = null;
            return (T)obj;
        }

        public void ReleaseObject()
        {
            CheckForPool();
            if (!pools.ContainsKey(poolID)) return;

            OnRelease?.Invoke(this);
            OnReleaseReset?.Invoke(this);
            pools[poolID].Release(this);
            gameObject.SetActive(false);
        }

        protected virtual void ResetObject() { }

        private void CheckForPool()
        {
            //short-circuit if object is not a prefab
            if (gameObject.GetInstanceID() < 0) return;

            poolID = gameObject.GetInstanceID();
            if (!pools.ContainsKey(poolID))
            {
                pools.Add(poolID, new Pool(this));
            }
        }
    }
}