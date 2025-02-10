using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using UnityEngine;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    [ShowInInspector, ReadOnly, FoldoutGroup("Effect Modifiers")]
    private List<EffectModifier> effectModifiers = new List<EffectModifier>();
    private Dictionary<EffectTag, float> precalculatedModifiers = new Dictionary<EffectTag, float>();

    public void AddModifier(EffectModifier modifier)
    {
        if (precalculatedModifiers.ContainsKey(modifier.effectTag))
            precalculatedModifiers.Remove(modifier.effectTag);
        effectModifiers.Add(modifier);
    }

    public void AddModifier(DamageType damageType, EffectTag tag, float magnitude, EffectModifierCalculationType type, float contributionMultiplier, Effect effect)
    {
        if (precalculatedModifiers.ContainsKey(tag))
            precalculatedModifiers.Remove(tag);
        switch (type)
        {
            case EffectModifierCalculationType.Flat:
                effectModifiers.Insert(0, new EffectModifier(damageType, tag, magnitude, type, contributionMultiplier, effect));
                break;
            case EffectModifierCalculationType.Additive:
            case EffectModifierCalculationType.Multiplicative:
                effectModifiers.Add(new EffectModifier(damageType, tag, magnitude, type, contributionMultiplier, effect));
                break;
        }
    }

    public void RemoveModifier(Effect effect)
    {
        effectModifiers.RemoveAll(x => x.effect == effect);
    }

    public void AddModifiersToCalculation(EffectModifier calculation, EffectTag tag)
    {
        foreach (EffectModifier modifier in effectModifiers)
        {
            if (modifier.effectTag != tag) continue;
            calculation.AddModifier(modifier);
        }
    }

    public float CalculateModifier(EffectTag tags, DamageType damage = DamageType.None)
    {
        float result = 0;
        if (precalculatedModifiers.ContainsKey(tags))
        {
            result = precalculatedModifiers[tags];
        }
        else
        {
            result = CalculateModifier(damage, tags, null, 0, 0);
            precalculatedModifiers.Add(tags, result);
        }
        return result;
    }

    public float CalculateModifier(Effect contributingEffect, EffectTag tag, float contributionMultiplier = 1, float mitigationContributionMultiplier = 1)
    {
        return CalculateModifier(DamageType.None, tag, contributingEffect, contributionMultiplier, mitigationContributionMultiplier);
    }

    private float CalculateModifier(DamageType damageType, EffectTag tag, Effect contributingEffect, float contributionMultiplier, float mitigationContributionMultiplier)
    {
        EffectModifier calculation = EffectModifier.CreateCalculation(contributingEffect, true, damageType);
        AddModifiersToCalculation(calculation, tag);
        if (Owner.Stat<Stat_PlayerOwner>().scaleWithPlayerCharacterModifiers && !Owner.Stat<Stat_PlayerOwner>().playerCharacter)
        {
            Owner.Stat<Stat_PlayerOwner>().playerEntity.Stat<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, tag);
        }
        return calculation.CalculateModifier(contributingEffect);
    }

    public override void OnDestroy()
    {
        effectModifiers.Clear();
        precalculatedModifiers.Clear();
    }
}