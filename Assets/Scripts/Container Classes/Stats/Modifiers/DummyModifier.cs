using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DummyModifier<T> : IDataContainer<T>, IModifier<T>
{
    [field: SerializeReference, FoldoutGroup("@GetType()")]
    public virtual T Value { get; protected set; }
    public IStatTag Tag => StatTag;
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag<T> StatTag { get; protected set; }
    public List<IDataContainer<T>> Modifiers => null;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; protected set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int MaxStacks { get; protected set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int StacksAdded { get; protected set; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; protected set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int TickDuration { get; protected set; }
    [field: SerializeField, FoldoutGroup("Modifier"), ReadOnly]
    public bool RefreshDuration { get; protected set; }


    public DummyModifier() { }

    public DummyModifier(T value = default, IStatTag<T> tag = default, Alignment alignment = Alignment.Neutral,
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

    public void AddModifier<Data>(Data modifier) where Data : IDataContainer
    {
        throw new NotImplementedException();
    }

    public void RemoveModifier(IDataContainer modifier)
    {
        throw new NotImplementedException();
    }

    public bool ContainsModifier(IDataContainer modifier, out int count)
    {
        throw new NotImplementedException();
    }
}