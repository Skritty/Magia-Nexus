using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Modifier_Priority<T> : Modifier<T>, IPriority
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public byte Priority { get; set; }

    public Modifier_Priority() { }

    public Modifier_Priority(T value = default, byte priority = 0, IStat<T> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        Priority = priority;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }
}