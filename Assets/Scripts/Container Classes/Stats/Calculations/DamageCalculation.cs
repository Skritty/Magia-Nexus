using System;

public interface IHasDamageTypes
{
    public DamageType AppliesTo { get; }
    public DamageType DamageType { get; }
}

[Serializable]
public class DamageCalculation : NumericalSolver
{
    public class Subcalculation : StepCalculation, IHasDamageTypes
    {
        public DamageType AppliesTo { get; }
        public DamageType DamageType { get; }
        public Subcalculation(DamageType appliesTo, DamageType damageType) : base()
        {
            AppliesTo = appliesTo;
            DamageType = damageType;
        }
    }

    /// <summary>
    /// Add a subcalculation to a root EffectModifier
    /// </summary>
    protected Subcalculation AddSubcalculation(DamageType damageType)
    {
        Subcalculation subcalculation = new Subcalculation(damageType, damageType);
        Modifiers.Add(subcalculation);
        return subcalculation;
    }

    public override void Add(IDataContainer<float> modifier)
    {
        CalculationStep step = CalculationStep.Flat;
        DamageType appliesTo = DamageType.True;
        DamageType damageType = DamageType.True;
        if (modifier is ICalculationComponent)
        {
            step = (modifier as ICalculationComponent).Step;
            if (modifier is IHasDamageTypes)
            {
                appliesTo = (modifier as IHasDamageTypes).AppliesTo;
                damageType = (modifier as IHasDamageTypes).DamageType;
            }
        }

        switch (step)
        {
            case CalculationStep.Flat:
                {
                    bool validModifier = appliesTo == DamageType.True ? true : false;
                    if (!validModifier)
                        foreach (Subcalculation subcalculation in Modifiers)
                        {
                            if (subcalculation.DamageType.HasFlag(appliesTo))
                            {
                                if (subcalculation.DamageType == appliesTo)
                                {
                                    (subcalculation.Modifiers[0] as Solver<float>).Modifiers.Add(modifier as IDataContainer<float>);
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
                        AddSubcalculation(damageType).Add(modifier);
                    }
                    break;
                }
            case CalculationStep.Additive:
                {
                    foreach (Subcalculation subcalculation in Modifiers)
                    {
                        if (appliesTo == DamageType.All || subcalculation.DamageType.HasFlag(appliesTo))
                        {
                            (subcalculation.Modifiers[1] as Solver<float>).Modifiers.Add(modifier as IDataContainer<float>);
                        }
                    }
                    break;
                }
            case CalculationStep.Multiplicative:
                {
                    foreach (Subcalculation subcalculation in Modifiers)
                    {
                        if (appliesTo == DamageType.All || subcalculation.DamageType.HasFlag(appliesTo))
                        {
                            subcalculation.Modifiers.Add(modifier as IDataContainer<float>);
                        }
                    }
                    break;
                }
        }

        changed = true;
    }
}