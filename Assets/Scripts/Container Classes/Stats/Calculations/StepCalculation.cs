using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[Serializable]
public class StepCalculation : NumericalSolver
{
    [ShowInInspector, FoldoutGroup("@GetType()")]
    public override float Value
    {
        get
        {
            if (changed)
            {
                Solve();
                changed = false;
            }
            return _value;
        }
        protected set
        {
            Modifiers.Clear();
            Modifiers.Add(new NumericalSolver(value, CalculationStep.Flat));
            Modifiers.Add(new NumericalSolver(CalculationStep.Additive));
            changed = true;
        }
    }

    public StepCalculation() 
    {
        Step = CalculationStep.Multiplicative;
        RefreshCalculation();
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public void RefreshCalculation()
    {
        Modifiers.Clear();
        Modifiers.Add(new NumericalSolver(CalculationStep.Flat));
        Modifiers.Add(new NumericalSolver(CalculationStep.Additive));
        changed = true;
    }

    public void AddModifier<Data>(Data modifier, CalculationStep step)
    {
        Solver<float> calcStep = this;
        switch (step)
        {
            case CalculationStep.Flat:
                {
                    calcStep = Modifiers[0] as Solver<float>;
                    break;
                }
            case CalculationStep.Additive:
                {
                    calcStep = Modifiers[1] as Solver<float>;
                    break;
                }
        }
        calcStep.Modifiers.Add(modifier as IDataContainer<float>);
        changed = true;
    }
    public override void Add(IDataContainer<float> modifier)
    {
        if(modifier is ICalculationComponent) AddModifier(modifier, (modifier as ICalculationComponent).Step);
        else AddModifier(modifier, CalculationStep.Flat);
    }
}

/*public class AssistContributingModifier : NumericalSolver TODO
{
    public float contributionMultiplier = 1;

    public AssistContributingModifier(IModifier<float> modifier, float contributionMultiplier)
    {
        this.Value = modifier.Value;
        this.source = modifier.source;
        this.contributionMultiplier = contributionMultiplier;
    }

    public override void InverseSolve()
    {
        if (source == null) return;
        source.Owner.GetMechanic<Stat_PlayerOwner>().ApplyContribution(source.Target, Value);
        //source.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(source.Target, absolute);
        *//*foreach (KeyValuePair<Entity, float> contributor2 in contributors)
        {
            if (contributor2.Value <= 0) continue;
            contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributor2.Key, -contributor.Value * contributor2.Value / totalPositiveContribution * contributingEffect.effectMultiplier);
        }*//*
    }
}*/