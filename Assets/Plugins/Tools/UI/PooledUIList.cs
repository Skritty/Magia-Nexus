// Created by: Julian Noel
using Skritty.Tools.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.UI
{
    public class PooledUIList : MonoBehaviour
    {
        public PooledObject pooledElementAsset;
        public Transform layoutGroup;

        [SerializeReference, ReadOnly]
        protected List<GameObject> elementList;

        /// <summary>
        /// Tries to add an object to the element list from the pool.
        /// </summary>
        /// <param name="element"></param>
        public GameObject Add()
        {
            GameObject newElement = pooledElementAsset.RequestObject();

            if(newElement != null)
            {
                newElement.transform.parent = layoutGroup;
                elementList.Add(newElement);
            }

            return newElement;
        }

        /// <summary>
        /// Tries to remove the object from the element list. Uses the pool if it can
        /// </summary>
        /// <param name="element"></param>
        public void Remove(GameObject element)
        {
            if(element != null && elementList.Contains(element))
            {
                PooledObject poolRef = element.GetComponent<PooledObject>();
                if(poolRef != null)
                {
                    poolRef.ReleaseObject();
                }
                else
                {
                    ConsoleLog.LogWarning("Failed to find PooledObject reference on supposedly pooled object! Destroying instead.");
                    Destroy(element);
                }

                elementList.Remove(element);
            }
        }
    }
}
