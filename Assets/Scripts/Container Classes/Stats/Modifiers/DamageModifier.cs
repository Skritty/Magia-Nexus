using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class DamageModifier : Stat<float>, IHasDamageTypes, ICalculationComponent, IModifier<float>
{
    public IStatTag Tag => StatTag;
    [field: SerializeReference, FoldoutGroup("Modifier"), HideInInlineEditors]
    public IStatTag<float> StatTag { get; protected set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int MaxStacks { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int StacksAdded { get; private set; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int TickDuration { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool RefreshDuration { get; private set; }
    [field: SerializeField, FoldoutGroup("@GetType()"), HideIf("@this is IStatTag")]
    public CalculationStep Step { get; private set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public DamageType AppliesTo { get; private set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public DamageType DamageType { get; private set; }

    public DamageModifier() { }

    public DamageModifier(float value = default, CalculationStep step = CalculationStep.Flat, DamageType appliesTo = DamageType.True, DamageType damageType = DamageType.True, IStatTag<float> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        _value = value;
        Step = step;
        AppliesTo = appliesTo;
        DamageType = damageType;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public DamageModifier(DamageType appliesTo, DamageType damageType, float baseValue)
    {
        this.AppliesTo = appliesTo;
        this.DamageType = damageType;
        _value = baseValue;
    }

    public override Stat Clone()
    {
        DamageModifier clone = (DamageModifier)base.Clone();
        clone.StatTag = StatTag;
        clone.Alignment = Alignment;
        clone.MaxStacks = MaxStacks;
        clone.StacksAdded = StacksAdded;
        clone.PerPlayer = PerPlayer;
        clone.TickDuration = TickDuration;
        clone.RefreshDuration = RefreshDuration;
        return clone;
    }
}
