//Created By: Julian Noel
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Skritty.Tools.Utilities
{

    /// <summary>
    /// Wrapper for WeightedChance that supports animation curve weights.
    /// <br/>
    /// Given a list of WeightedCurveChanceEntry<T> and an input value to evaluate the curves at, 
    /// calculates and returns a random entry value using each value's weight curves.
    /// </summary>
    /// <typeparam name="T">Type of object to be returned.</typeparam>
    public class WeightedCurveChance<T>
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.TableList]
#endif
        private List<WeightedCurveChanceEntry<T>> entries;

        //------------------------Private Fields------------------------//

        private Dictionary<T, WeightedCurveChanceEntry<T>> _entryMap = new Dictionary<T, WeightedCurveChanceEntry<T>>();

        //------------------------Public Methods------------------------//

        #region Constructors
        public WeightedCurveChance()
        {
            entries = new List<WeightedCurveChanceEntry<T>>();
        }

        public WeightedCurveChance(List<WeightedCurveChanceEntry<T>> entryList)
        {
            entries = entryList;

            foreach (WeightedCurveChanceEntry<T> entry in entries)
            {
                _entryMap[entry.value] = entry;
            }
        }
        #endregion

        #region Add Overloads

        /// <summary>
        /// Adds a new <see cref="WeightedChanceEntry{T}"/> entry object built from the inputs.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="weight">An <see cref="AnimationCurve"/> describing the "continuous" set of weights for this entry.</param>
        public virtual void Add(T item, AnimationCurve weight)
        {
            WeightedCurveChanceEntry<T> newEntry = new WeightedCurveChanceEntry<T>(weight, item);
            Add(newEntry);
        }

        /// <summary>
        /// Adds the given <see cref="WeightedChanceEntry{T}"/> by reference.
        /// </summary>
        /// <param name="newEntry">A prebuilt <see cref="WeightedChanceEntry{T}"/> containing a value T and its corresponding weight curve.</param>
        public void Add(WeightedCurveChanceEntry<T> newEntry)
        {
            entries.Add(newEntry);
        }

        #endregion Add Overloads

        #region Get Overloads

        /// <summary>
        /// Wrapper for WeightedChance.GetRandomEntry() that grabs the required curve input first.
        /// <br/>
        /// Gets a weighted random entry from the available entries, using a RANDOM seed.
        /// </summary>
        /// <returns>Returns a weighted random entry.</returns>
        public T GetRandomEntry(float input)
        {
            System.Random random = new System.Random();
            return GetRandomEntry(random, input);
        }

        /// <summary>
        /// Wrapper for WeightedChance.GetRandomEntry() that grabs the required curve input first.
        /// <br/>
        /// Gets a weighted random entry from the available entries, using a GIVEN seed.
        /// </summary>
        /// <param name="random">Random seed to use when selecting an entry.</param>
        /// <returns>Returns a weighted random entry.</returns>
        public T GetRandomEntry(System.Random random, float input)
        {
            List<WeightedChanceEntry<T>> generatedEntries = entries.Select(
                entry => new WeightedChanceEntry<T>(entry.GetWeightAt(input), entry.value)
            ).ToList();

            WeightedChance<T> weightedChances = new WeightedChance<T>(generatedEntries);

            return weightedChances.GetRandomEntry(random);
        }

        #endregion Get Overloads

        /// <summary> Determines whether or not the specified object can be found here.</summary>
        public bool ContainsEntry(T item) => _entryMap.TryGetValue(item, out _);

        #region Operators

        /// <summary>
        /// Enables using [] syntax to retrieve or add WeightedChanceEntry objects. Does not support removal.
        /// </summary>
        public WeightedCurveChanceEntry<T> this[int index] => entries[index];


        /// <summary>
        /// Enables using [] syntax to retrieve or add WeightedChanceEntry objects. Does not support removal.
        /// </summary>
        public WeightedCurveChanceEntry<T> this[T item] => _entryMap[item];

        #endregion
    }

    public class WeightedCurveChanceEntry<T>
    {
        public AnimationCurve weightCurve;
        public T value;

        public WeightedCurveChanceEntry(AnimationCurve curve, T val)
        {
            if (curve.length < 1 || curve.keys.Min(key => key.value) < 0f)
            {
                throw new InvalidOperationException($"Invalid data in AnimationCurve {curve}. " +
                    $"Must have at least one keyframe, and none of those keyframes can be negative.");
            }

            weightCurve = curve;
            value = val;
        }

        /// <summary>
        /// Evaluates the curve at the given input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float GetWeightAt(float input)
        {
            return weightCurve.Evaluate(input);
        }
    }
}
