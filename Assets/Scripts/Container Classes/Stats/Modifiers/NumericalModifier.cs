using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : Modifier<float>, ICalculationComponent
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public CalculationStep Step { get; set; }
    
    public NumericalModifier() { }
    public NumericalModifier(float value = default, CalculationStep step = CalculationStep.Flat, IStat<float> tag = default, Alignment alignment = Alignment.Neutral,
        int maxStacks = 0, int stacksAdded = 1, bool perPlayer = false,
        int tickDuration = 0, bool refreshDuration = false)
    {
        Value = value;
        Step = step;
        StatTag = tag;
        Alignment = alignment;
        MaxStacks = maxStacks;
        StacksAdded = stacksAdded;
        PerPlayer = perPlayer;
        TickDuration = tickDuration;
        RefreshDuration = refreshDuration;
    }

    public NumericalModifier Clone()
    {
        NumericalModifier clone = (NumericalModifier)MemberwiseClone();
        clone.Value = Value;
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
