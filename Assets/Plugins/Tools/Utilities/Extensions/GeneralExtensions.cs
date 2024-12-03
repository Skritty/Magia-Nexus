// Created by: Trevor T.
// Edited by: Bill D.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

namespace Skritty.Tools.Utilities
{
    public static class GeneralExtensions
    {
        /// <summary>
        /// Will backfill the reference if it is empty. Updates the actual given reference and also returns it for convenience;
        /// </summary>
        /// <typeparam name="T">The type of Component. Must extend from Component</typeparam>
        /// <param name="gameObject"></param>
        /// <param name="componentContainer"></param>
        /// <returns></returns>
        public static T GetComponentIfNull<T>(this GameObject gameObject, ref T componentContainer) where T : Component
        {
            if(componentContainer == null)
            {
                componentContainer = gameObject.GetComponent<T>();
            }

            return componentContainer;
        }

        /// <summary>
        /// Like TryGetComponent but only writes out to the ref variable if it isn't already null.
        /// </summary>
        /// <typeparam name="T">The type of the component you want</typeparam>
        /// <param name="gameObject"></param>
        /// <param name="componentContainer">the container for the component you're trying to fill</param>
        /// <returns>true if a component was found (be it from already having the ref or from the internal TryGetComponent succeeding), false if not</returns>
        public static bool TryGetComponentIfNull<T>(this Component gameObject, ref T componentContainer) where T : Component
        {
            return componentContainer != null || gameObject.TryGetComponent<T>(out componentContainer);
        }

        /// <summary>
        /// Will backfill the reference if it is empty. Updates the actual given reference and also returns it for convenience;
        /// </summary>
        /// <typeparam name="T">The type of Component. Must extend from Component</typeparam>
        /// <param name="gameObject"></param>
        /// <param name="componentContainer"></param>
        /// <returns></returns>
        public static T GetComponentIfNull<T>(this Component gameObject, T componentContainer) where T : Component
        {
            if(componentContainer == null)
            {
                componentContainer = gameObject.GetComponent<T>();
            }

            return componentContainer;
        }

        /// <summary>
        /// Like TryGetComponent but only writes out to the ref variable if it isn't already null.
        /// </summary>
        /// <typeparam name="T">The type of the component you want</typeparam>
        /// <param name="gameObject"></param>
        /// <param name="componentContainer">the container for the component you're trying to fill</param>
        /// <returns>true if a component was found (be it from already having the ref or from the internal TryGetComponent succeeding), false if not</returns>
        public static bool TryGetComponentIfNull<T>(this GameObject gameObject, ref T componentContainer) where T : Component
        {
            return componentContainer != null || gameObject.TryGetComponent<T>(out componentContainer);
        }

        public static void CopyComponentValues<T>(this T self, T other) where T : Component
        {
            foreach(PropertyInfo property in self.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                if(!property.CanWrite || !property.CanRead)
                    continue;

                property.SetValue(self, property.GetValue(other));
            }
        }

        public static void CopyComponentValues<T>(this T self, T other, string[] exclude) where T : Component
        {
            foreach(PropertyInfo property in self.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                if(!property.CanWrite || !property.CanRead || exclude.Contains(property.Name))
                    continue;

                property.SetValue(self, property.GetValue(other));
            }
        }

        public static Texture2D toTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }

        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                      TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }

        public static void DoFunctionToTree(this Transform root, System.Action<Transform> function)
        {
            function.Invoke(root);
            foreach(Transform child in root)
                child.DoFunctionToTree(function);
        }

        public static void RunFunctionOnDelay(this MonoBehaviour mb, System.Action function, YieldInstruction delay)
        {
            IEnumerator DoFunction()
            {
                yield return delay;
                function.Invoke();
            }
            mb.StartCoroutine(DoFunction());
        }

        public static void RunFunctionOnDelay(this MonoBehaviour mb, System.Action function, CustomYieldInstruction delay)
        {
            IEnumerator DoFunction()
            {
                yield return delay;
                function.Invoke();
            }
            mb.StartCoroutine(DoFunction());
        }

        public static bool IsBitSet(this int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
        public static void ToggleActive(this GameObject gameObject)
        {
            if(gameObject.activeSelf)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }

        public static bool TryGetIndexOf<T>(this List<T> list, T element, out int outputIndex)
        {
            outputIndex = list.IndexOf(element);
            return outputIndex >= 0;
        }

        /// <summary>
        /// Like LINQ's ElementAtOrDefault but gives null instead of a default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object ElementAtOrNull<T>(this List<T> list, int index) where T : class
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Like LINQ's ElementAtOrDefault but gives null instead of a default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryGetElementAt<T>(this List<T> list, int index, out T outputElement)
        {
            ConsoleLog.Log("trying to get element, received adjusted index => " + index);

            bool indexIsValid = index >= 0 && index < list.Count;
            
            ConsoleLog.Log("index >= 0 => " + (index >= 0));
            ConsoleLog.Log("index < list.Count => " + (index < list.Count));

            outputElement = indexIsValid ? list[index] : default(T);

            ConsoleLog.Log("tryget success = " + indexIsValid);

            return indexIsValid;
        }

        #region Enum Movements
        /// <summary>
        /// Grabs the next valid enum value. If looping is disabled and it goes out of bound, it returns the original input (bc structs can't be null).
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Next<SomeEnum>(this SomeEnum sourceEnumValue, bool loopAtEnd=false) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            //get the array index of enum value that's right after it in the list
            //This is NOT the actual integer associated with the enum value itself, just the index in orderedEnumValues
            int nextEnumIndex = Array.IndexOf<SomeEnum>(orderedEnumValues, sourceEnumValue) + 1;

            if (nextEnumIndex == orderedEnumValues.Length) //if out of bounds...
            {
                //loop around or return original input
                return (loopAtEnd) ? orderedEnumValues[0] : sourceEnumValue;
            }
            else
            {
                return orderedEnumValues[nextEnumIndex];
            }
        }

        /// <summary>
        /// Wrapper for enum Next() function that tries to determine if a next value was actually found
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <param name="nextValue"></param>
        /// <param name="loopAtEnd"></param>
        /// <returns></returns>
        public static bool TryGetNext<SomeEnum>(this SomeEnum sourceEnumValue, out SomeEnum nextValue, bool loopAtEnd = false) where SomeEnum : System.Enum
        {
            nextValue = sourceEnumValue.Next(false);
            return !nextValue.Equals(sourceEnumValue); //if the next value is the same, it did not advance. Failed to actually get next
        }

        /// <summary>
        /// Grabs the previous valid enum value. If looping is disabled and it goes out of bound, it returns the original input (bc structs can't be null).
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Prev<SomeEnum>(this SomeEnum sourceEnumValue, bool loopAtStart = false) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            //get the array index of enum value that's right after it in the list
            //This is NOT the actual integer associated with the enum value itself, just the index in orderedEnumValues
            int nextEnumIndex = Array.IndexOf<SomeEnum>(orderedEnumValues, sourceEnumValue) - 1;

            if(nextEnumIndex < 0) //if out of bounds...
            {
                //loop around or return original input
                return (loopAtStart) ? orderedEnumValues.Last() : sourceEnumValue;
            }
            else
            {
                return orderedEnumValues[nextEnumIndex];
            }
        }

        /// <summary>
        /// Wrapper for enum Prev() function that tries to determine if a previous value was actually found
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <param name="nextValue"></param>
        /// <param name="loopAtEnd"></param>
        /// <returns></returns>
        public static bool TryGetPrev<SomeEnum>(this SomeEnum sourceEnumValue, out SomeEnum nextValue, bool loopAtEnd = false) where SomeEnum : System.Enum
        {
            nextValue = sourceEnumValue.Prev(false);
            return !nextValue.Equals(sourceEnumValue); //if the next value is the same, it did not advance. Failed to actually get next
        }

        /// <summary>
        /// Grabs the first valid enum value. Due to explicit enum values, this cannot be assumed to simply be the enum value at 0
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum First<SomeEnum>(this SomeEnum sourceEnumValue) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            return orderedEnumValues[0];
        }

        /// <summary>
        /// Grabs the last valid enum value.
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Last<SomeEnum>(this SomeEnum sourceEnumValue) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            return orderedEnumValues.Last();
        }

        #endregion Enum Movements

        #region DeepClear
        /// <summary>
        /// Bottom Recursive Layer for recursively running DeepClear() on nested collections.
        /// </summary>
        public static void DeepClear<T>(this ICollection<T> collection)
        {
            collection.Clear();
        }

        /// <summary>
        /// Recursively runs Clear on nested collections.
        /// </summary>
        public static void DeepClear<T>(this ICollection<ICollection<T>> collection)
        {
            foreach (ICollection<T> item in collection)
            {
                item.DeepClear();
            }

            collection.Clear();
        }

        /// <summary>
        /// KeyValuePair overload for recursively running Clear() on nested collections.
        /// </summary>
        public static void DeepClear<T, U>(this ICollection<KeyValuePair<T, ICollection<U>>> dictionary)
        {
            foreach (KeyValuePair<T, ICollection<U>> item in dictionary)
            {
                item.Value.DeepClear();
            }

            dictionary.Clear();
        }

        #endregion DeepClear

        /// <summary>
        /// Fully clears the array like you would a List or IEnumerable.
        /// </summary>
        /// <remarks>
        /// Wasn't able to find a Clear extension method for arrays, and System.Array.Clear() is needlessly annoying for clearing the whole array.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void Clear<T>(this T[] array)
        {
            System.Array.Clear(array, 0, array.Length);
        }
    }
}
