using System;
using Sirenix.OdinInspector;

[Serializable]
public class DamageSolver : NumericalSolver
{
    [FoldoutGroup("@GetType()")]
    public DamageType appliesTo;
    [FoldoutGroup("@GetType()"), HideIf("@step != CalculationStep.Flat")]
    public DamageType damageType;

    public DamageSolver() { }

    public DamageSolver(float value, CalculationStep method, DamageType damageType, DamageType appliesTo) : base(value, method)
    {
        this.damageType = damageType;
        this.appliesTo = appliesTo;
    }
}