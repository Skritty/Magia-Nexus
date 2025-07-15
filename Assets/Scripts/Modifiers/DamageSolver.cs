using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class DamageSolver : NumericalSolver
{
    [HideInInspector]
    public Dictionary<DamageType, float> finalDamage;
    public DamageType appliesTo;
    [HideIf("@method != NumericalModifierCalculationMethod.Flat")]
    public DamageType damageType;

    public DamageSolver() { }

    public DamageSolver(float value, NumericalModifierCalculationMethod method, DamageType damageType, DamageType appliesTo) : base(value, method)
    {
        this.damageType = damageType;
        this.appliesTo = appliesTo;
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public new static DamageSolver CreateCalculation()
    {
        return new DamageSolver(1, true);
    }
    public DamageSolver(int baseValue, bool root = false) : base(baseValue, root) { }

    public void AddModifier(IModifier<float> modifier, NumericalModifierCalculationMethod method, DamageType damageType, DamageType appliesTo)
    {
        if(method == NumericalModifierCalculationMethod.Flat)
        {
            bool validModifier = appliesTo == DamageType.True ? true : false;
            if(!validModifier)
                foreach (DamageSolver subcalculation in Modifiers[0].Modifiers)
                {
                    if (subcalculation.damageType.HasFlag(appliesTo))
                    {
                        if(subcalculation.damageType == damageType)
                        {
                            subcalculation.AddModifier(modifier, method);
                            validModifier = false;
                            break;
                        }
                        {
                            validModifier = true;
                        }
                    }
                }
            
            if (validModifier)
            {
                AddSubcalculation(damageType).AddModifier(modifier, method);
            }
        }
        else
        {
            foreach (DamageSolver subcalculation in Modifiers[0].Modifiers)
            {
                if (appliesTo == DamageType.All || subcalculation.damageType.HasFlag(appliesTo))
                {
                    subcalculation.AddModifier(modifier, method);
                }
            }
        }
    }

    /// <summary>
    /// Add a subcalculation to a root EffectModifier
    /// </summary>
    /// <param name="tags"></param>
    protected DamageSolver AddSubcalculation(DamageType damageType)
    {
        DamageModifier subcalculation = new DamageModifier(damageType, 1);
        Modifiers[0].Modifiers.Add(subcalculation);
        return subcalculation;
    }

    public DamageSolver Clone()
    {
        return (DamageSolver)MemberwiseClone();
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