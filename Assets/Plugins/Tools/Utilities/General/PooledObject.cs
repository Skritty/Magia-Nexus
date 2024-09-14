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
            public GameObject prefab;
            Queue<GameObject> pool = new Queue<GameObject>();
            int maxSize;
            int currentSize = 0;
            int avaliable = 0;
            public bool ObjectAvaliable
            {
                get
                {
                    return pool.Count > 0 || currentSize < maxSize;
                }
            }

            public Pool(int maxSize, GameObject prefab)
            {
                this.maxSize = maxSize;
                this.prefab = prefab;
            }

            public GameObject Get()
            {
                GameObject obj = null;
                if (pool.Count > 0)
                {
                    obj = pool.Dequeue();
                    if (obj == null) obj = Get();
                    if (obj == null) return null;
                    avaliable--;
                    obj.SetActive(true);
                }
                else if (currentSize < maxSize)
                {
                    currentSize++;
                    obj = Instantiate(prefab);
                }
                //obj = Instantiate(prefab);
                return obj;
            }

            public void Release(GameObject obj)
            {
                pool.Enqueue(obj);
                //Debug.Log($"Pool size: {pool.Count} - {currentSize}/{maxSize}");
                //Destroy(obj);
                //currentSize--;
                avaliable++;
            }
        }

        [SerializeField]
        private int maximumPoolSize = 20;
        [SerializeField, HideInInspector]
        private int poolID;

        public GameObject Prefab => pools[poolID].prefab;
        public bool ObjectAvaliable => pools.ContainsKey(poolID) ? pools[poolID].ObjectAvaliable : true;
        public UnityEvent<GameObject> OnGet;
        public System.Action<GameObject> OnGetReset;
        public UnityEvent<GameObject> OnRelease;
        public System.Action<GameObject> OnReleaseReset;

        public GameObject RequestObject()
        {
            CheckForPool();
            GameObject obj = pools[poolID].Get();
            obj.GetComponent<PooledObject>().ResetObject();
            OnGet?.Invoke(obj);
            OnGetReset?.Invoke(obj);
            OnReleaseReset = null;
            OnGetReset = null;
            return obj;
        }

        public void ReleaseObject()
        {
            CheckForPool();
            if (!pools.ContainsKey(poolID)) return;

            OnRelease?.Invoke(gameObject);
            OnReleaseReset?.Invoke(gameObject);
            pools[poolID].Release(gameObject);
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
                pools.Add(poolID, new Pool(maximumPoolSize, gameObject));
            }
        }
    }
}