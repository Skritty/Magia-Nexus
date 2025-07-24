using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : NumericalSolver, IModifier<float>
{
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag Tag { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; private set; }
    [field: SerializeField, FoldoutGroup("@Modifier")]
    public int MaxStacks { get; private set; }
    [field: SerializeField, FoldoutGroup("@Modifier")]
    public int StacksAdded { get; private set; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int TickDuration { get; private set; }
    [field: ShowInInspector, FoldoutGroup("Modifier")]
    public bool RefreshDuration { get; private set; }

    public NumericalModifier() { }
    public NumericalModifier(float value = default, CalculationStep step = CalculationStep.Flat, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        bool temporary = false, int tickDuration = 0, bool refreshDuration = false)
    {
        _value = value;
        base.step = step;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public override Stat Clone()
    {
        NumericalModifier clone = (NumericalModifier)base.Clone();
        clone.Tag = Tag;
        clone.Alignment = Alignment;
        clone.MaxStacks = MaxStacks;
        clone.StacksAdded = StacksAdded;
        clone.PerPlayer = PerPlayer;
        clone.TickDuration = TickDuration;
        clone.RefreshDuration = RefreshDuration;
        return clone;
    }
}
