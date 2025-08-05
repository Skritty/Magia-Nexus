using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Modifier<T> : IModifier<T>
{
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public virtual T Value { get; set; }
    public IStat Tag => StatTag;
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public IStat<T> StatTag { get; set; }
    public List<IDataContainer<T>> Modifiers => null;
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public Alignment Alignment { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int MaxStacks { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int StacksAdded { get; protected set; } = 1;
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public bool PerPlayer { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public int TickDuration { get; protected set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public bool RefreshDuration { get; protected set; }


    public Modifier() { }

    public Modifier(T value = default, IStat<T> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public bool Get<T1>(out T1 data)
    {
        IDataContainer<T1> container = (IDataContainer<T1>)this;
        if (container == null) data = default;
        else data = container.Value;
        return container != null;
    }
}

public class UnityObjectModifier<T> : Modifier<T>
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public override T Value { get; set; }

    public UnityObjectModifier(T value = default, IStat<T> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }
}