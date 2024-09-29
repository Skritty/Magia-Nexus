using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    public class EffectModifier
    {
        public Effect effect;
        public EffectModifierCalculationType type;
        public EffectTag tag;
        public EffectTag grantedTag;
        public float modifier;

        public float buffedModifier;
        public float positive;
        public float negative;
        public List<EffectModifier> additive = new List<EffectModifier>();// At most 1 additive if there are multipliers
        public List<EffectModifier> multipliers = new List<EffectModifier>();

        public EffectModifier(Effect effect = null, EffectModifierCalculationType type = EffectModifierCalculationType.Flat, EffectTag tag = EffectTag.None, EffectTag grantedTag = EffectTag.None, float modifier = 0)
        {
            this.effect = effect;
            this.type = type;
            this.tag = tag;
            this.grantedTag = grantedTag;
            this.modifier = modifier;
        }

        public EffectModifier GetSubcalculation(EffectTag tags)
        {
            EffectModifier subcalculation = null;
            foreach (EffectModifier calculation in additive)
            {
                if (tags == calculation.tag)
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
            EffectModifier subcalculation = GetSubcalculation(modifier.tag);
            if (subcalculation == null)
            {
                subcalculation = new EffectModifier();
                subcalculation.tag = modifier.tag;
                subcalculation.additive.Add(new EffectModifier());
                subcalculation.multipliers.Add(new EffectModifier());
                additive.Add(subcalculation);
            }
            subcalculation.additive[0].additive.Add(modifier);
            return subcalculation;
        }

        public void AddAdditive(EffectModifier modifier, EffectModifier subcalculation)
        {
            subcalculation.multipliers[0].additive.Add(modifier);
        }

        public void AddMultiplicative(EffectModifier modifier, EffectModifier subcalculation)
        {
            subcalculation.multipliers.Add(modifier);
        }

        public float CalculateModifier(Effect contributingEffect, float contributionMultiplier, float negativeContributionMultiplier)
        {
            Solve();
            if (contributingEffect != null && (contributionMultiplier != 0 && negativeContributionMultiplier != 0))
                Contribute(modifier * contributionMultiplier, (buffedModifier - modifier) * negativeContributionMultiplier, positive, negative, contributingEffect);
            return modifier;
        }

        private float Solve()
        {
            if (modifier < 0)
            {
                negative = modifier;
            }
            else
            {
                positive = modifier;
            }
            foreach (EffectModifier damageCalculation in additive)
            {
                float amt = damageCalculation.Solve();
                modifier += amt;
                if (modifier < 0)
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
                modifier *= 1 + amt;
                if (modifier < 0)
                {
                    negative += damageCalculation.negative;
                }
                else
                {
                    buffedModifier *= 1 + amt;
                    positive += damageCalculation.positive;
                }
            }
            return modifier;
        }

        private void Contribute(float damageDealt, float damageMitigated, float totalPositive, float totalNegative, Effect contributingEffect)
        {
            if (effect != null && effect.Owner && modifier != 0)
            {
                if (positive == 0) damageDealt = 0;
                else damageDealt = damageDealt / totalPositive * positive;

                if (negative == 0) damageMitigated = 0;
                else damageMitigated = damageMitigated / totalNegative * negative;

                if (modifier > 0)
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
    public void AddModifier(Effect effect, EffectModifierCalculationType type, EffectTag tag, EffectTag grantedTag, float modifier)
    {
        effectModifiers.Add(new EffectModifier(effect, type, tag, grantedTag, modifier));
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
                rootCalculation.AddFlat(new EffectModifier(null, EffectModifierCalculationType.Flat, EffectTag.None | additionalRequiredTags, EffectTag.None, 1));
            }
            else
            {
                rootCalculation.multipliers.Add(new EffectModifier(contributingEffect, EffectModifierCalculationType.Multiplicative, EffectTag.None, EffectTag.None, contributingEffect.effectMultiplier - 1));
                foreach (KeyValuePair<EffectTag, float> tag in contributingEffect.effectTags)
                {
                    EffectModifier subcalculation = rootCalculation.AddFlat(new EffectModifier(contributingEffect, EffectModifierCalculationType.Flat, tag.Key, tag.Key, 1));
                    rootCalculation.AddMultiplicative(new EffectModifier(contributingEffect, EffectModifierCalculationType.Multiplicative, tag.Key, tag.Key, tag.Value - 1), subcalculation);
                }
            }
        }

        for(int i = 0; i < rootCalculation.additive.Count; i++)
        {
            EffectModifier subcalculation = rootCalculation.additive[i];
            HashSet<EffectModifier> flatGained = new HashSet<EffectModifier>();
            foreach (EffectModifier modifier in effectModifiers)
            {
                if(!flatGained.Contains(modifier) && (modifier.tag & (subcalculation.tag | additionalRequiredTags)) == (subcalculation.tag | additionalRequiredTags))
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
                                flatGained.Add(modifier);
                                rootCalculation.AddFlat(new EffectModifier(modifier.effect, modifier.type, modifier.grantedTag, modifier.grantedTag, modifier.modifier));
                                break;
                            }
                    }
                }
            }
        }
        // (fire, spell) from (attack)
        // (1 (slashing, attack)) * 1.5 + (1 (fire, spell) from buff) * 1.5
        // 1 * 1.5 + 1 * 1.5 = 3, (1 + 1) * 1.5 = 3
        return rootCalculation;
    }
}

[Flags]
public enum EffectTag
{
    None          = 0,

    // Damage Types
    // Physical
    Bludgeoning   = 1 << 0, // Knocks back
    Slashing      = 1 << 1, // Bleeds (scales off of base slashing damage)
    Piercing      = 1 << 2, // +1 Piereces (Projectiles)/Hits Extra Targets (AoE, but reduced AoE Cone, longer AoE)
    // Elemental
    Fire          = 1 << 3, // Ignites (scales off of base fire damage)
    Lightning     = 1 << 4, // +1 Chains
    Cold          = 1 << 5, // Slows
    //Special
    Magical       = 1 << 6, // Cannot be blocked
    Chaos         = 1 << 7, // More damage per debuff on target
    Healing       = 1 << 8, // More healing per buff on target

    // Sources
    Attack        = 1 << 9,
    Spell         = 1 << 10,
    DoT           = 1 << 11, // Gains projectile behaviors (chain, multiple projectiles)
    Projectile    = 1 << 12,

    // Effects
    DamageDealt   = 1 << 13,
    DamageTaken   = 1 << 14,
    AoE           = 1 << 15,
    Targets       = 1 << 16,
    Slow          = 1 << 17,
    Knockback     = 1 << 18,
    MovementSpeed = 1 << 19,
    Initiative    = 1 << 20,

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