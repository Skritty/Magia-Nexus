using Skritty.Tools.Utilities;
using System;
using System.Collections.Generic;

namespace Skritty.Other
{
    public class CleanableHashSet<T> : HashSet<T>, ICleanable
    {
        public Action ChangedEvent { get; set; } = null;
        public bool IsDirty { get; protected set; } = true;

        public void Clean()
        {
            IsDirty = false;
        }

        public new bool Add(T item) => MarkDirtyIf(base.Add(item));
        public new bool Remove(T item) => MarkDirtyIf(base.Remove(item));

        public new void ExceptWith(IEnumerable<T> other)
        {
            int oldCount = Count;
            base.ExceptWith(other);
            MarkDirtyIf(oldCount != Count);
        }

        public new void Clear()
        {
            int oldCount = Count;
            base.Clear();
            MarkDirtyIf(oldCount != Count);
        }

        public new int RemoveWhere(Predicate<T> match)
        {
            int succ = base.RemoveWhere(match);
            MarkDirtyIf(succ > 0);
            return succ;
        }

        //TODO: implement the other hashset edit functions after confirming whether or not they already call Add or Remove()
        //public new void IntersectWith(IEnumerable<T> other);
        //public void TrimExcess();
        //public void UnionWith(IEnumerable<T> other);

        public override int GetHashCode() => HashCode.Combine(Comparer, Count, ChangedEvent, IsDirty);

        /// <summary>
        /// Marks the HashSet as dirty if the condition is true. Returns said condition for cleaner syntax/compactness.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private bool MarkDirtyIf(bool condition)
        {
            if(condition)
            {
                IsDirty = true;
                ChangedEvent?.Invoke();
            }
            return condition;
        }
    }
}
