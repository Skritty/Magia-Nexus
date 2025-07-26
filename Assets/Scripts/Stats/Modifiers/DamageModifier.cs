using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageModifier : DamageSolver, IModifier<float>
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
    [field: ShowInInspector, FoldoutGroup("Modifier")]
    public bool RefreshDuration { get; private set; }

    public DamageModifier() { }

    public DamageModifier(DamageType damageType, float baseValue)
    {
        this.damageType = damageType;
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
