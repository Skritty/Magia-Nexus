using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Modifier_Numerical : Modifier<float>, ICalculationComponent
{
    [field: SerializeField, FoldoutGroup("@GetType()")]
    public CalculationStep Step { get; set; }
    
    public Modifier_Numerical() { }
    public Modifier_Numerical(float value = default, CalculationStep step = CalculationStep.Flat, IStat<float> tag = default, Alignment alignment = Alignment.Neutral,
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

    public Modifier_Numerical Clone()
    {
        Modifier_Numerical clone = (Modifier_Numerical)MemberwiseClone();
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
