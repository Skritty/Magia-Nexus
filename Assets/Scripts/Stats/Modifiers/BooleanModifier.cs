using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BooleanModifier : BooleanPrioritySolver, IModifier<bool>
{
    public BooleanModifier() { }
    public EffectTask Source { get; set; }
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag Tag { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int MaxStacks { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int StacksAdded { get; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; }
    [field: ShowInInspector, FoldoutGroup("Modifier")]
    public int TickDuration { get; }
    [field: ShowInInspector, FoldoutGroup("Modifier"), ReadOnly]
    public bool RefreshDuration { get; }

    public BooleanModifier(bool value = default, CalculationStep step = CalculationStep.Flat, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        bool temporary = false, int tickDuration = 0, bool refreshDuration = false)
    {
        _value = value;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }
}