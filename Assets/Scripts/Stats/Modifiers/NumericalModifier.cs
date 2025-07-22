using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : NumericalSolver, IModifier<float>
{
    public EffectTask Source { get; set; }
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag Tag { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; }
    [field: SerializeField, FoldoutGroup("@Modifier")]
    public int MaxStacks { get; }
    [field: SerializeField, FoldoutGroup("@Modifier")]
    public int StacksAdded { get; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int TickDuration { get; }
    [field: ShowInInspector, FoldoutGroup("Modifier"), ReadOnly]
    public bool RefreshDuration { get; }

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
}
