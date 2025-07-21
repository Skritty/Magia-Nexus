using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : NumericalSolver, IModifier<float>
{
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; } = 1;
    public bool PerPlayer { get; }
    public bool Temporary { get; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int TickDuration { get; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public bool RefreshDuration { get; }

    public NumericalModifier() { }
    public NumericalModifier(float value = default, CalculationStep step = CalculationStep.Flat, IStatTag tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        bool temporary = false, int tickDuration = 0, bool refreshDuration = false)
    {
        _value = value;
        Step = step;
        Tag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        Temporary = temporary;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }
}
