using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    public class EffectModifier
    {
        public Effect effect;
        public EffectModifierType type;
        public EffectTag tag;
        public float modifier;

        public float buffedModifier;
        public float positive;
        public float negative;
        public List<EffectModifier> additive = new List<EffectModifier>();// At most 1 additive if there are multipliers
        public List<EffectModifier> multipliers = new List<EffectModifier>();

        public EffectModifier(bool root, Effect effect = null, EffectModifierType type = EffectModifierType.Base, EffectTag tag = EffectTag.None, float modifier = 0)
        {
            this.effect = effect;
            this.type = type;
            this.tag = tag;
            this.modifier = modifier;
            if (root)
            {
                additive.Add(new EffectModifier(false));
                multipliers.Add(new EffectModifier(false));
            }
        }

        public void AddBase(EffectModifier modifier)
        {
            additive[0].additive.Add(modifier);
        }

        public void AddAdditive(EffectModifier modifier)
        {
            multipliers[0].additive.Add(modifier);
        }

        public void AddMultiplicative(EffectModifier modifier)
        {
            multipliers.Add(modifier);
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
                if (modifier > 0)
                {
                    effect.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Target, damageDealt);
                }
                else if (contributingEffect != effect)
                {
                    effect.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Owner, damageMitigated);
                }
            }

            if (positive == 0) damageDealt = 0;
            else damageDealt = damageDealt / totalPositive * positive;

            if (negative == 0) damageMitigated = 0;
            else damageMitigated = damageMitigated / totalNegative * negative;

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
    public Dictionary<Effect, EffectModifier> effectModifier = new Dictionary<Effect, EffectModifier>();
    public void AddMultiplier(Effect effect, EffectModifierType type, EffectTag tag, float modifier)
    {
        effectModifier.Add(effect, new EffectModifier(false, effect, type, tag, modifier));
    }

    public void RemoveMultiplier(Effect effect)
    {
        effectModifier.Remove(effect);
    }

    public float CalculateModifier(params EffectTag[] tags)
    {
        return CalculateModifier(null, 0, 0, tags);
    }

    public float CalculateModifier(Effect contributingEffect, float contributionMultiplier, float negativeContributionMultiplier, params EffectTag[] tags)
    {
        return CreateCalculation(null, contributingEffect, tags).CalculateModifier(contributingEffect, contributionMultiplier, negativeContributionMultiplier);
    }

    public EffectModifier CreateCalculation(EffectModifier calculation, Effect contributingEffect, params EffectTag[] tags)
    {
        if (calculation == null)
        {
            calculation = new EffectModifier(true);
            if (contributingEffect == null)
            {
                calculation.AddBase(new EffectModifier(false, null, EffectModifierType.Base, EffectTag.Global, 1));
            }
            else
            {
                calculation.AddBase(new EffectModifier(false, contributingEffect, EffectModifierType.Base, contributingEffect.Tags, 1));
                calculation.AddBase(new EffectModifier(false, contributingEffect, EffectModifierType.Multiplicative, contributingEffect.Tags, contributingEffect.baseEffectMultiplier - 1));
            }
        }

        foreach (KeyValuePair<Effect, EffectModifier> kvp in effectModifier)
        {
            EffectModifier modifier = kvp.Value;
            bool valid = true;
            foreach (EffectTag tag in tags)
            {
                if ((modifier.tag & tag) == 0)
                {
                    valid = false;
                }
            }
            if (valid)
            {
                switch (modifier.type)
                {
                    case EffectModifierType.Multiplicative:
                        {
                            calculation.AddMultiplicative(modifier);
                            break;
                        }
                    case EffectModifierType.Additive:
                        {
                            calculation.AddAdditive(modifier);
                            break;
                        }
                    case EffectModifierType.Base:
                        {
                            calculation.AddBase(modifier);
                            break;
                        }
                }
            }
        }
        return calculation;
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

public enum EffectModifierType
{
    Base,
    Additive,
    Multiplicative
}