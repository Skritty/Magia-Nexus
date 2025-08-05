using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Modifier_Damage : Modifier<float>, IHasDamageTypes, ICalculationComponent 
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public CalculationStep Step { get; private set; }
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public DamageType AppliesTo { get; private set; }
    [field: SerializeField, FoldoutGroup("@GetType()"), HideIf("@Step != CalculationStep.Flat")]
    public DamageType DamageType { get; private set; }

    public Modifier_Damage() { }

    public Modifier_Damage(float value = default, CalculationStep step = CalculationStep.Flat, DamageType appliesTo = DamageType.True, DamageType damageType = DamageType.True, IStat<float> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
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

    public Modifier_Damage(DamageType appliesTo, DamageType damageType, float baseValue)
    {
        this.AppliesTo = appliesTo;
        this.DamageType = damageType;
        Value = baseValue;
    }

    public Modifier_Damage Clone()
    {
        Modifier_Damage clone = (Modifier_Damage)MemberwiseClone();
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
