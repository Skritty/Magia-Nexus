using System;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[Serializable]
public class NumericalStepCalculationSolver : NumericalSolver
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
            Modifiers.Add(new NumericalSolver(CalculationStep.Flat));
            Modifiers.Add(new NumericalSolver(CalculationStep.Additive));
            (Modifiers[0] as Stat<float>)?.AddModifier(new DataContainer<float>(value));
            changed = true;
        }
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public NumericalStepCalculationSolver()
    {
        step = CalculationStep.Multiplicative;
        // Flat
        Modifiers.Add(new NumericalSolver(CalculationStep.Flat));
        // Increased
        Modifiers.Add(new NumericalSolver(CalculationStep.Additive));
        // More
        /*if (source != null) TODO: add source
        {
            modifiers.Add(new NumericalModifier(source.effectMultiplier, source));
        }*/
    }

    public override void AddModifier<Data>(Data modifier)
    {
        Stat<float> calcStep = this;
        if(modifier is NumericalSolver)
            switch ((modifier as NumericalSolver).step)
            {
                case CalculationStep.Flat:
                    {
                        calcStep = Modifiers[0] as Stat<float>;
                        break;
                    }
                case CalculationStep.Additive:
                    {
                        calcStep = Modifiers[1] as Stat<float>;
                        break;
                    }
            }
        calcStep.Modifiers.Add(modifier as IDataContainer<float>);
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