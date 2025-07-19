using Sirenix.OdinInspector;
using UnityEngine;

public class NumericalModifier : NumericalSolver, IDuration, IModifier<float>
{
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; set; }
    public Alignment Alignment { get; set; }
    public bool Temporary { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int CurrentTick { get; set; }
    [field: ShowIf("@Temporary")]
    public int TickDuration { get; set; } = -1;
    [field: ShowIf("@Temporary")]
    public bool RefreshDuration { get; set; }

    public NumericalModifier() { }

    public NumericalModifier(float baseValue, CalculationStep calculationType)
    {
        _value = baseValue;
        this.Step = calculationType;
    }
}
