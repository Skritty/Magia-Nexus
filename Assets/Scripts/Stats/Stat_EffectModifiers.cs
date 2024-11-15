using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    public class EffectModifier
    {
        public Effect effect;
        public EffectModifierCalculationType type;
        public (EffectTag, float)[] effectTags; // (tag, magnitude)

        public EffectTag Tag => effectTags[0].Item1;
        public float magnitude;
        public float buffedModifier;
        public float positive;
        public float negative;
        public List<EffectModifier> additive = new List<EffectModifier>();// At most 1 additive if there are multipliers
        public List<EffectModifier> multipliers = new List<EffectModifier>();

        public EffectModifier(Effect effect = null, EffectModifierCalculationType type = EffectModifierCalculationType.Flat, params (EffectTag, float)[] effectTags)
        {
            this.effect = effect;
            this.type = type;
            this.effectTags = effectTags;
        }

        public EffectModifier GetSubcalculation(EffectTag tags)
        {
            EffectModifier subcalculation = null;
            foreach (EffectModifier calculation in additive)
            {
                if (tags == calculation.Tag)
                {
                    subcalculation = calculation;
                    break;
                }
            }
            return subcalculation;
        }

        public EffectModifier AddFlat(EffectModifier modifier)
        {
            // Root: ((damage type subcalculation) + ...)
            // Damage type subcalculation: ((Flat + ...) * (Increased + ...) * Multiplier * ...)
            // TODO: Do damage taken on second step
            EffectModifier subcalculation = GetSubcalculation(modifier.Tag);
            if (subcalculation == null)
            {
                subcalculation = new EffectModifier();
                subcalculation.effectTags = modifier.effectTags;
                subcalculation.multipliers.Add(new EffectModifier(modifier.effect, EffectModifierCalculationType.Additive, modifier.effectTags));
                additive.Add(subcalculation);
            }
            modifier.magnitude = modifier.effectTags[0].Item2;
            subcalculation.additive.Add(modifier);
            return subcalculation;
        }

        public void AddAdditive(EffectModifier modifier, EffectModifier subcalculation)
        {
            modifier.magnitude = modifier.effectTags[0].Item2;
            subcalculation.multipliers[0].additive.Add(modifier);
        }

        public void AddMultiplicative(EffectModifier modifier, EffectModifier subcalculation)
        {
            modifier.magnitude = modifier.effectTags[0].Item2 - 1;
            subcalculation.multipliers.Add(modifier);
        }

        public float CalculateModifier(Effect contributingEffect, float contributionMultiplier, float negativeContributionMultiplier)
        {
            Solve();
            if (contributingEffect != null && (contributionMultiplier != 0 && negativeContributionMultiplier != 0))
                Contribute(magnitude * contributionMultiplier, (buffedModifier - magnitude) * negativeContributionMultiplier, positive, negative, contributingEffect);
            return magnitude;
        }

        private float Solve()
        {
            if (magnitude < 0)
            {
                negative = magnitude;
            }
            else
            {
                positive = magnitude;
            }
            foreach (EffectModifier damageCalculation in additive)
            {
                float amt = damageCalculation.Solve();
                magnitude += amt;
                if (magnitude < 0)
                {
                    negative += damageCalculation.negative;
                }
                else
                {
                    buffedModifier += amt;
                    positive += damageCalculation.positive;
                }
            }
            foreach (EffectModifier damageCalculation in multipliers)
            {
                float amt = damageCalculation.Solve();
                magnitude *= 1 + amt;
                if (magnitude < 0)
                {
                    negative += damageCalculation.negative;
                }
                else
                {
                    buffedModifier *= 1 + amt;
                    positive += damageCalculation.positive;
                }
            }
            return magnitude;
        }

        private void Contribute(float damageDealt, float damageMitigated, float totalPositive, float totalNegative, Effect contributingEffect)
        {
            if (effect != null && effect.Owner && magnitude != 0)
            {
                if (positive == 0) damageDealt = 0;
                else damageDealt = damageDealt / totalPositive * positive;

                if (negative == 0) damageMitigated = 0;
                else damageMitigated = damageMitigated / totalNegative * negative;

                if (magnitude > 0)
                {
                    effect.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Target, damageDealt);
                }
                else if (contributingEffect != effect)
                {
                    effect.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Owner, damageMitigated);
                }
            }

            foreach (EffectModifier damageCalculation in additive)
            {
                damageCalculation.Contribute(damageDealt, damageMitigated, totalPositive, totalNegative, contributingEffect);
            }
            foreach (EffectModifier damageCalculation in multipliers)
            {
                damageCalculation.Contribute(damageDealt, damageMitigated, totalPositive, totalNegative, contributingEffect);
            }
        }
    }

    [ShowInInspector, ReadOnly, FoldoutGroup("Effect Modifiers")]
    public List<EffectModifier> effectModifiers = new List<EffectModifier>();
    public void AddModifier(Effect effect, EffectModifierCalculationType type, float modifier)
    {
        switch (type)
        {
            case EffectModifierCalculationType.Flat:
                effectModifiers.Insert(0, new EffectModifier(effect, type, effect.effectTags.Select(x => (x.Key, x.Value)).ToArray()));
                break;
            default:
                effectModifiers.Add(new EffectModifier(effect, type, effect.effectTags.Select(x => (x.Key, x.Value)).ToArray()));
                break;
        }
        
    }

    public void RemoveModifier(Effect effect)
    {
        effectModifiers.RemoveAll(x => x.effect == effect);
    }

    public float CalculateModifier(EffectTag additionalRequiredTags = EffectTag.None)
    {
        return CalculateModifier(null, 0, 0, additionalRequiredTags);
    }

    public float CalculateModifier(Effect contributingEffect, float contributionMultiplier, float negativeContributionMultiplier, EffectTag additionalRequiredTags = EffectTag.None)
    {
        return CreateCalculation(null, contributingEffect, additionalRequiredTags).CalculateModifier(contributingEffect, contributionMultiplier, negativeContributionMultiplier);
    }

    public EffectModifier CreateCalculation(EffectModifier rootCalculation, Effect contributingEffect, EffectTag additionalRequiredTags)
    {
        if (rootCalculation == null)
        {
            rootCalculation = new EffectModifier();
            if (contributingEffect == null)
            {
                rootCalculation.AddFlat(new EffectModifier(null, EffectModifierCalculationType.Flat, (EffectTag.None | additionalRequiredTags, 1)));
            }
            else
            {
                rootCalculation.AddMultiplicative(new EffectModifier(contributingEffect, EffectModifierCalculationType.Multiplicative, (EffectTag.None, contributingEffect.effectMultiplier)), rootCalculation);
                foreach (KeyValuePair<EffectTag, float> tag in contributingEffect.effectTags)
                {
                    EffectModifier subcalculation = rootCalculation.AddFlat(new EffectModifier(contributingEffect, EffectModifierCalculationType.Flat, (tag.Key, 1)));
                    rootCalculation.AddMultiplicative(new EffectModifier(contributingEffect, EffectModifierCalculationType.Multiplicative, (tag.Key, tag.Value)), subcalculation);
                }
            }
        }

        foreach (EffectModifier modifier in effectModifiers)
        {
            foreach ((EffectTag, float) effectTag in modifier.effectTags)
            {
                if (!effectTag.Item1.HasFlag(additionalRequiredTags)) continue; 
                EffectModifier flat = null;
                for (int i = 0; i < rootCalculation.additive.Count; i++)
                {
                    EffectModifier subcalculation = rootCalculation.additive[i]; 
                    if (subcalculation.Tag.HasFlag(effectTag.Item1 & ~(additionalRequiredTags)))
                    {
                        switch (modifier.type)
                        {
                            case EffectModifierCalculationType.Multiplicative:
                                {
                                    subcalculation.AddMultiplicative(modifier, subcalculation);
                                    break;
                                }
                            case EffectModifierCalculationType.Additive:
                                {
                                    subcalculation.AddAdditive(modifier, subcalculation);
                                    break;
                                }
                            case EffectModifierCalculationType.Flat:
                                {
                                    if (flat == null)
                                    {
                                        flat = new EffectModifier(modifier.effect, modifier.type, effectTag);
                                    }
                                    else
                                    {
                                        flat.effectTags[0].Item1 |= effectTag.Item1;
                                    }
                                    break;
                                }
                        }
                    }
                }
                if(flat != null)
                {
                    rootCalculation.AddFlat(flat);
                }
            }
        }
        return rootCalculation;
    }
}

[Flags]
public enum EffectTag
{
    // Parent Types
    None          = 0, // 0000
    Damage        = 1, // 0000
    Physical      = 2, // 1000
    Elemental     = 4, // 0100
    Divine        = 8, // 0010

    // Physical
    Bludgeoning   = (1 << 4) + 1 + 2, // Knocks back
    Slashing      = (1 << 5) + 1 + 2, // Bleeds (scales off of base slashing damage)
    Piercing      = (1 << 6) + 1 + 2, // +1 Piereces (Projectiles)/Hits Extra Targets (AoE, but reduced AoE Cone, longer AoE)
    // Elemental
    Fire          = (1 << 7) + 1 + 4, // Ignites (scales off of base fire damage)
    Lightning     = (1 << 8) + 1 + 4, // +1 Chains
    Cold          = (1 << 9) + 1 + 4, // Slows
    // Divine
    Magical       = (1 << 10) + 1 + 8, // Cannot be blocked
    Chaos         = (1 << 11) + 1 + 8, // More damage per debuff on target
    Healing       = (1 << 12) + 8, // More healing per buff on target
    // Sources
    Attack        = 1 << 13,
    Spell         = 1 << 14,
    DoT           = 1 << 15, // Gains projectile behaviors (chain, multiple projectiles)
    Projectile    = 1 << 16,

    // Effects
    DamageDealt   = 1 << 17,
    DamageTaken   = 1 << 18,
    AoE           = 1 << 19,
    Targets       = 1 << 20,
    Removeable    = 1 << 21,
    Knockback     = 1 << 22,
    MovementSpeed = 1 << 23,
    Initiative    = 1 << 24,

    Global        = int.MaxValue
}

public enum EffectModifierCalculationType
{
    Flat,
    Additive,
    Multiplicative
}

// Damage Type Needs
// DamageInstance must support multiple damage type values (Give it a dictionary<damagetype, float> with a default value)
// Each damage type value is buffed by different things during damage calculation ("subcalculations" / find modifiers per damagetype)
// Activate triggers per damage type (done in damageinstance)

// RF (Fire, Burning, DoT) <- affected by DoT targeting support gem (grants Duration tag)