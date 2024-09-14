using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Interface for checking if a class has been changed (dirtied), enabling you to act upon it being updated
    /// </summary>
    public interface ICleanable
    {
        public Action ChangedEvent { get; set; }
        public bool IsDirty { get; }
        public void Clean();
    }

    /// <summary>
    /// Interface for checking if a class has been changed (dirtied), enabling you to act upon it being updated
    /// </summary>
    public interface ICleanable<T>
    {
        public Action<T> ChangedEvent { get; set; }
        public bool IsDirty { get; }
        public void Clean();
    }
}
