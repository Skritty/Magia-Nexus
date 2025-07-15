using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : NumericalSolver, IModifier<float>
{
    public NumericalModifier() { }
    public NumericalModifier(NumericalModifierCalculationMethod calculationType, float baseValue)
    {
        this.Method = calculationType;
        Value = baseValue;
        /*if (baseValue != 0)
            Modifiers.Add(new NumericalModifier(baseValue));*/
    }
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; set; }
    public int MaxStacks { get; set; } = -1;
    public int Stacks { get; set; } = 1;
    public bool PerPlayer { get; set; }
    public Alignment Alignment { get; set; }
    public bool Temporary { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int Tick { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int TickDuration { get; set; } = -1;
    [field: ShowIf("@Temporary"), ReadOnly]
    public bool RefreshDuration { get; set; }
}
