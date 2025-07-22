using Sirenix.OdinInspector;

public class DamageTypeCalculationSolver : NumericalStepCalculationSolver
{
    [FoldoutGroup("@GetType()")]
    public DamageType damageType;

    /// <summary>
    /// Root calculation
    /// </summary>
    public DamageTypeCalculationSolver()
    {
        step = CalculationStep.Flat;
    }

    /// <summary>
    /// Numerical calculation-type constructor
    /// </summary>
    private DamageTypeCalculationSolver(DamageType damageType) : base()
    {
        this.damageType = damageType;
    }

    /// <summary>
    /// Add a subcalculation to a root EffectModifier
    /// </summary>
    protected NumericalStepCalculationSolver AddSubcalculation(DamageType damageType)
    {
        DamageTypeCalculationSolver subcalculation = new DamageTypeCalculationSolver(damageType);
        Modifiers.Add(subcalculation);
        return subcalculation;
    }

    public override void AddModifier<Data>(Data modifier)
    {
        Stat<float> calcStep = this;
        DamageType appliesTo = DamageType.True;
        CalculationStep step = CalculationStep.Multiplicative;
        if (modifier is NumericalSolver)
        {
            step = (modifier as NumericalSolver).step;
            if (modifier is DamageSolver)
            {
                appliesTo = (modifier as DamageSolver).appliesTo;
            }
        }

        switch (step)
        {
            case CalculationStep.Flat:
                {
                    bool validModifier = appliesTo == DamageType.True ? true : false;
                    if (!validModifier)
                        foreach (DamageTypeCalculationSolver subcalculation in (Modifiers[0] as Stat<float>).Modifiers)
                        {
                            if (subcalculation.damageType.HasFlag(appliesTo))
                            {
                                if (subcalculation.damageType == appliesTo)
                                {
                                    (subcalculation.Modifiers[0] as Stat<float>).Modifiers.Add(modifier as IDataContainer<float>);
                                    validModifier = false;
                                    break; // Applies to the first valid flat damage only
                                }
                                {
                                    validModifier = true;
                                }
                            }
                        }

                    if (validModifier)
                    {
                        AddSubcalculation(appliesTo).AddModifier(modifier);
                    }
                    break;
                }
            case CalculationStep.Additive:
                {
                    foreach (DamageTypeCalculationSolver subcalculation in (Modifiers[0] as Stat<float>).Modifiers)
                    {
                        if (appliesTo == DamageType.All || subcalculation.damageType.HasFlag(appliesTo))
                        {
                            (subcalculation.Modifiers[1] as Stat<float>).Modifiers.Add(modifier as IDataContainer<float>);
                        }
                    }
                    break;
                }
            case CalculationStep.Multiplicative:
                {
                    foreach (DamageTypeCalculationSolver subcalculation in (Modifiers[0] as Stat<float>).Modifiers)
                    {
                        if (appliesTo == DamageType.All || subcalculation.damageType.HasFlag(appliesTo))
                        {
                            subcalculation.Modifiers.Add(modifier as IDataContainer<float>);
                        }
                    }
                    break;
                }
        }
    }
}