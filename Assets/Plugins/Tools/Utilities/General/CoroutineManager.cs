using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// An empty singleton for handling coroutines!
    /// </summary>
    public class CoroutineManager : Singleton<CoroutineManager> { }

    public static class CoroutineManagerExtensions
    {
        /// <summary>
        /// Starts a Coroutine
        /// </summary>
        public static Coroutine StartCoroutine(this object obj, IEnumerator routine)
        {
            if(CoroutineManager.Instance == null)
            {
                new GameObject().AddComponent<CoroutineManager>();
            }
            return CoroutineManager.Instance.StartCoroutine(routine);
        }

        public static void StopCoroutine(this object obj, Coroutine coroutine)
        {
            if(CoroutineManager.Instance == null)
            {
                new GameObject().AddComponent<CoroutineManager>();
            }
            CoroutineManager.Instance.StopCoroutine(coroutine);
        }
    }
}