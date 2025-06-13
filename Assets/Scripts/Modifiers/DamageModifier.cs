using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class DamageModifier : NumericalModifier
{
    public Dictionary<DamageType, float> finalDamage;
    public DamageType appliesTo;
    [HideIf("@method != NumericalModifierCalculationMethod.Flat")]
    public DamageType damageType;

    public DamageModifier() { }

    public DamageModifier(float value, NumericalModifierCalculationMethod method, DamageType damageType, DamageType appliesTo) : base(value, method)
    {
        this.damageType = damageType;
        this.appliesTo = appliesTo;
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public new static DamageModifier CreateCalculation(Effect source = null)
    {
        return new DamageModifier(1, source, true);
    }
    public DamageModifier(int baseValue, Effect source = null, bool root = false) : base(baseValue, source, root) { }

    public void AddModifier(Modifier<float> modifier, NumericalModifierCalculationMethod method, DamageType damageType, DamageType appliesTo)
    {
        if(method == NumericalModifierCalculationMethod.Flat)
        {
            bool validModifier = appliesTo == DamageType.True ? true : false;
            if(!validModifier)
                foreach (DamageModifier subcalculation in submodifiers[0].submodifiers)
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
            foreach (DamageModifier subcalculation in submodifiers[0].submodifiers)
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
    protected DamageModifier AddSubcalculation(DamageType damageType)
    {
        DamageModifier subcalculation = new DamageModifier(1, null, true);
        subcalculation.damageType = damageType;
        submodifiers[0].AddModifier(subcalculation);
        return subcalculation;
    }

    public DamageModifier Clone()
    {
        return (DamageModifier)MemberwiseClone();
    }
}

public class AssistContributingModifier : NumericalModifier
{
    public float contributionMultiplier = 1;

    public AssistContributingModifier(Modifier<float> modifier, float contributionMultiplier)
    {
        this.value = modifier.value;
        this.source = modifier.source;
        this.contributionMultiplier = contributionMultiplier;
    }

    public override void InverseSolve()
    {
        if (source == null) return;
        source.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(source.Target, value);
        //source.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(source.Target, absolute);
        /*foreach (KeyValuePair<Entity, float> contributor2 in contributors)
        {
            if (contributor2.Value <= 0) continue;
            contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributor2.Key, -contributor.Value * contributor2.Value / totalPositiveContribution * contributingEffect.effectMultiplier);
        }*/
    }

}