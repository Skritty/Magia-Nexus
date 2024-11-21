using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix.Models.Charity;
using UnityEngine;
using UnityEngine.UIElements;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    public class EffectModifier
    {
        public EffectModifierCalculationType type;
        public EffectTag tags;
        public float magnitude;
        public Effect effect;

        public List<EffectModifier> submodifiers = new List<EffectModifier>();

        // For calculating contribution
        public float contributionMagnitude; // magnitude including contribution multiplier 100x * 0x
        public float positive; // magnitude including contribution multiplier, without negative modifiers
        public float negative;
        private void SetAllMagnitudes(float amount, float contributionMultiplier)
        {
            magnitude = amount;
            
            switch (type)
            {
                case EffectModifierCalculationType.Flat:
                    contributionMagnitude = magnitude * contributionMultiplier;
                    positive = magnitude > 0 ? contributionMagnitude : 0;
                    negative = magnitude < 0 ? Mathf.Abs(contributionMagnitude) : 0;
                    break;
                case EffectModifierCalculationType.Additive:
                    contributionMagnitude = magnitude * contributionMultiplier;
                    positive = magnitude > 0 ? contributionMagnitude : 0;
                    negative = magnitude < 0 ? Mathf.Abs(contributionMagnitude) : 0;
                    break;
                case EffectModifierCalculationType.Multiplicative:
                case EffectModifierCalculationType.FlatMultiplicative:
                    contributionMagnitude = 1 + (magnitude - 1) * contributionMultiplier;
                    positive = magnitude > 0 ? contributionMagnitude : 1;
                    negative = magnitude < 0 ? Mathf.Abs(contributionMagnitude - 1) : 0;
                    break;
            }
        }

        /// <summary>
        /// Create a new root EffectModifier
        /// </summary>
        public EffectModifier()
        {
            SetAllMagnitudes(1, 1);
            type = EffectModifierCalculationType.Multiplicative;
            EffectModifier subcalculations = new EffectModifier(EffectModifierCalculationType.Flat);
            //subcalculations.type = EffectModifierCalculationType.Additive;
            submodifiers.Add(subcalculations);
        }

        protected EffectModifier(EffectModifierCalculationType type)
        {
            this.type = type;
            if(type != EffectModifierCalculationType.Flat)
            {
                SetAllMagnitudes(1, 1);
            }
        }

        // Leaf node (contains a magnitude)
        public EffectModifier(EffectTag tags, float magnitude, EffectModifierCalculationType type, float contributionMultiplier, Effect effect)
        {
            this.type = type;
            this.tags = tags;
            SetAllMagnitudes(magnitude, contributionMultiplier);
            this.effect = effect;
        }

        
        /// <summary>
        /// Add a subcalculation to a root EffectModifier
        /// </summary>
        /// <param name="tags"></param>
        public EffectModifier AddSubcalculation(EffectTag tags)
        {
            EffectModifier subcalculation = new EffectModifier(EffectModifierCalculationType.FlatMultiplicative);
            subcalculation.tags = tags;
            submodifiers[0].submodifiers.Add(subcalculation);
            // Flat
            subcalculation.submodifiers.Add(new EffectModifier(EffectModifierCalculationType.Flat));
            // Additive
            subcalculation.submodifiers.Add(new EffectModifier(EffectModifierCalculationType.Additive));
            return subcalculation;
        }

        // Dealing 1 (physical, peircing, projectile) 1 (fire)

        // +1 flat added (physical, damageDealt)
        // (physical, damageDealt) HAS (damageDealt) ? YES
        // (physical, peircing, projectile) HAS (physical)
        // subcalc:(physical, peircing, projectile) - DOUBLE DIPPING SOMETIMES
        // 1 + 1
        // subcalc:(physical, peircing, projectile) | subcalc:(physical) - OWN SUBCALC, SCALES ONLY WITH PHYSICAL
        // 1                                          1
        protected EffectModifier FindSubcalculation(EffectTag modifierTags, EffectTag additionalRequiredTags)
        {
            foreach (EffectModifier subcalculation in submodifiers[0].submodifiers)
            {
                if (!modifierTags.HasFlag(additionalRequiredTags)) continue;
                if (subcalculation.tags.HasFlag(modifierTags & ~(additionalRequiredTags)))
                {
                    return subcalculation;
                }
            }
            return null;
        }

        public bool TryAddFlat(EffectModifier modifier, EffectTag additionalRequiredTags = EffectTag.None, Effect effect = null)
        {
            EffectModifier subcalculation = FindSubcalculation(modifier.tags, additionalRequiredTags);
            if (subcalculation != null)
            {
                subcalculation.submodifiers[0].submodifiers.Add(modifier);
            }
            else
            {
                //AddSubcalculation(modifier.tags).submodifiers[0].submodifiers.Add(modifier);
            }
            return false;
        }

        public bool TryAddAdditive(EffectModifier modifier, EffectTag additionalRequiredTags)
        {
            EffectModifier subcalculation = FindSubcalculation(modifier.tags, additionalRequiredTags);
            if (subcalculation != null)
            {
                subcalculation.submodifiers[1].submodifiers.Add(modifier);
            }
            return false;
        }

        public bool TryAddMultiplicative(EffectModifier modifier, EffectTag additionalRequiredTags)
        {
            EffectModifier subcalculation = FindSubcalculation(modifier.tags, additionalRequiredTags);
            if (subcalculation != null)
            {
                subcalculation.submodifiers.Add(modifier);
            }
            return false;
        }

        public float CalculateModifier(Effect contributingEffect)
        {
            Solve();
            Contribute(contributingEffect);
            return magnitude;
        }

        private void Solve()
        {
            // NODE
            // 1. root (4 * 2) = 8
            // 2. subcalcs (4 + ...) ...
            // 3. subcalc1 (1 * [2] * [1] * [2]) = 4
            // 4. flat (1 + 1)
            // 5. leaf modifier ...
            // 6. increased (1 + 0.5 - 0.5)
            // 7. leaf modifier ...
            // 8. more (leaf modifier) ... (2)
            // 9. effect multi (2)

            foreach (EffectModifier modifier in submodifiers)
            {
                modifier.Solve();

                switch (type)
                {
                    case EffectModifierCalculationType.Flat:
                        {
                            magnitude += modifier.magnitude;
                            contributionMagnitude += modifier.contributionMagnitude;
                            positive += modifier.positive;
                            negative += modifier.negative;
                            break;
                        }
                    case EffectModifierCalculationType.Additive:
                        {
                            magnitude += modifier.magnitude;
                            contributionMagnitude += modifier.contributionMagnitude;
                            positive += modifier.positive;
                            negative += modifier.negative;
                            break;
                        }
                    case EffectModifierCalculationType.Multiplicative:
                    case EffectModifierCalculationType.FlatMultiplicative:
                        {
                            magnitude *= modifier.magnitude;
                            contributionMagnitude *= modifier.contributionMagnitude;
                            positive *= modifier.positive;
                            negative += modifier.negative; 
                            break;
                        }
                }
            }

            if(submodifiers.Count > 0)
            {
                switch (type)
                {
                    case EffectModifierCalculationType.Flat:
                        {
                            negative = Mathf.Clamp01(negative / positive);
                            break;
                        }
                    case EffectModifierCalculationType.Additive:
                        {
                            negative = Mathf.Clamp01(negative);
                            break;
                        }
                    case EffectModifierCalculationType.Multiplicative:
                    case EffectModifierCalculationType.FlatMultiplicative:
                        {
                            negative = Mathf.Clamp01(negative);
                            break;
                        }
                }
            }
        }

        // (-1 ABS in setup) 1 * 0.5 * 0.5 = 0.25, 2 
        // Total = 1, Max = 2, Mitigated = 1
        // (root)[2, 0.25], Positive:1*2/2=1, Negative:1*0.25/0.25=1
        // (flat)[2, 0] * (inc)[0, 0.5] * 0+[0.5], Positive:1*2/2=1, Negative1:1*0.5/1=0.5, Negative2:1*0.5/1=0.5
        // 0+[1]+[1] 1+[-0.25]+[-.25], Negative1:0.5*0.25/0.5=0.25

        // 0.5x, 1+(0.5-1)*2=0-1
        // 0x 0.5x sum = -1-0.5= -1.5
        // 1*-1/1.5

        // Total 0, Max 2, Mitigated 2
        // [4,1]
        // [2,1] + [2,1]
        // [1,0.5] *               [0,1]              .5/1.5=1/3, 2/3
        // 0+[+1]+[0.25]+[0.25]   0+[0.5]+[1]    
        // .25/.5

        //
        // (root) [4,1 OR 1.5/4 CORRECT: 2/3 ]
        // tags: [2,.5] + [2,1] calculate negative individually

        // More
        // *1.2 (0.2) = *20% more
        // *0.85 (-.15) = *85% less

        // Increased
        // +0.2 = *+20% increased
        // -0.15 = *-15% decreased

        // Flat
        // +1 = 1 added flat
        // -1 = -1 subtracted flat

        // P1[team1] (1 damage) is buffed by P2[team1] (doubles damage) but P3[team2, target] has 50% less damage taken = they take 1 damage

        // (root)
        // 1*(subcalcs)*[effect multi]
        // 0+(movement speed)[1.1]
        // 1*(flat)[1] * (increase)[0.1+1]
        // 0+[1]        0+[0.1]

        private void Contribute(Effect contributingEffect)
        {
            if (contributingEffect == null) return;

            Dictionary<Entity, float> contributors = new Dictionary<Entity, float>();
            CalculateContribution(contributors, contributionMagnitude, positive - contributionMagnitude);

            // Version 2
            float totalPositiveContribution = 0;
            foreach(KeyValuePair<Entity, float> contributor in contributors)
            {
                if(contributor.Value >= 0)
                {
                    totalPositiveContribution += contributor.Value;
                }
            }

            // Hand out contributions
            foreach(KeyValuePair<Entity, float> contributor in contributors)
            {
                if(contributor.Value >= 0)
                {
                    contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Target, contributor.Value * contributingEffect.effectMultiplier);
                }
                else
                {
                    // Version 1: All mitigated goes towards effect owner
                    //contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Owner, contributor.Value * contributingEffect.effectMultiplier);

                    // Version 2: Mitigated is split between contributors to effect (not just effect owner, but buff owners too)
                    foreach (KeyValuePair<Entity, float> contributor2 in contributors)
                    {
                        if (contributor2.Value <= 0) continue;
                        contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributor2.Key, -contributor.Value * contributor2.Value / totalPositiveContribution * contributingEffect.effectMultiplier);
                    }
                }
            }
        }
        private void CalculateContribution(Dictionary<Entity, float> contributors, float totalMagnitude, float mitigatedMagnitude)
        {
            // Is leaf
            if (effect != null && effect.Owner)
            {
                if (magnitude >= 0)
                {
                    if(!contributors.TryAdd(effect.Owner, totalMagnitude))
                    {
                        contributors[effect.Owner] += totalMagnitude;
                    }
                }
                else //if(effect.Owner.Stat<Stat_Team>().team != contributingEffect.Owner.Stat<Stat_Team>().team)
                {
                    if (!contributors.TryAdd(effect.Owner, -mitigatedMagnitude))
                    {
                        contributors[effect.Owner] -= mitigatedMagnitude;
                    }
                }
            }

            float sum = 0;
            float negativeSum = 0;
            foreach (EffectModifier modifier in submodifiers)
            {
                switch (modifier.type)
                {
                    case EffectModifierCalculationType.Flat:
                    case EffectModifierCalculationType.FlatMultiplicative:
                        sum += modifier.positive;
                        negativeSum += modifier.negative;
                        break;
                    case EffectModifierCalculationType.Additive:
                    case EffectModifierCalculationType.Multiplicative:
                        sum += modifier.positive - 1;
                        negativeSum += modifier.negative;
                        break;
                }
            }
            foreach (EffectModifier modifier in submodifiers)
            {
                switch (modifier.type)
                {
                    case EffectModifierCalculationType.Flat:
                    case EffectModifierCalculationType.FlatMultiplicative:
                        modifier.CalculateContribution(contributors, totalMagnitude * modifier.positive / sum, mitigatedMagnitude * modifier.negative / negativeSum);
                        break;
                    case EffectModifierCalculationType.Additive:
                    case EffectModifierCalculationType.Multiplicative:
                        modifier.CalculateContribution(contributors, totalMagnitude * (modifier.positive - 1) / sum, mitigatedMagnitude * modifier.negative / negativeSum);
                        break;
                }
            }
        }
    }

    // Damage calculation 
    // Root: ((damage type subcalculation) + ...)
    // Damage type subcalculation: ((Flat + ...) * (Increased + ...) * Multiplier * ...)

    //                                                      (ROOT REAL)
    // n0 Multiplicative: --------------------------(root)------------------------------ * (effect multiplier)(-)
    // n1 Additive:       -----------------------(tag subcalc)------------------------------ + ...
    // n2 Multiplicative: ---(flat)---    *    ---(increased)---(-)     * (more)(-) * (more)(-) * ...
    // n3 Additive:   [(f1) + (f2) + ...]     [(i1) + (i2) + ...]
    // n4 --------: leaf node, use value and settings

    // Nodes can only be additive or multiplicative, not both
    // Changes nothing for contibution calc, only how final number is calculated
    // contribution = [total for this group] * [magnitude] / [sub multi sum]

    // Damage Calc ((base + flat) * increased/reduced * more/less) ->(passed as NOT float) triggers, life lost

    // Total final amount, amount without negatives
    // (root) [68]
    // (projectile damage) [85]       * (effect multiplier)
    // (flat) * (increase) * (more)         (-.2) [.8]
    // (1)  (-0.15) [0.85]  (99) [100]

    // 2*3 = 6, 6 * 2/5 = 2.4 and 6 * 3/5 = 3.6
    // (root) total:[4.8]
    // (projectile damage) flat:[2.618]   * (effect multiplier) multi:[2.181]
    // (flat) *    (increase)[.685] * (more)[1.14]       (2)[2.181]
    // (1) + (1)          (1 + .2)       (100)
    //  X     P2            P1             X             P1[2.4]

    // Calc for total amount
    // Total = 1, Max = 3, Negative = -1 Mitigated = 2
    // (root)[1, 1] * [1 effect multi]X 2*-1/-1=2
    // (flat)[2, 0.8, ] * (increased)[-0.5, 0.2] * [100]*0, Total:   1*2/2.5=[0.8], 1*0.5/2.5=[0.2]
    // [1]+[1]        [-0.5] + [-0.5] + [0.5] 2*-0.5/-1=1               Maximum: 3*2/3.5=[1.71], 3*1.5/3.5=[1.28]
    // 0.8*1/2=0.4          0.2*.5/.5=0.2                           Mitigated = 0.91 Mitigated = 1.08

    // Calc for mitigated amounts
    // Total = 1, Max = 3
    // (1)*(root)[1,] * [1 effect multi]X
    // (1)*(flat)[2] * (increased)[-0.5] * [100]*0, 
    // (0)+[1]+[1]   (1)+[-0.5] + [-0.5] + [0.5]
    // no negatives       1*-0.5/-1=0.5

    // First squish contributions into each player in Dictionary
    // Normalize each contributed total amount into a %, multiply into amount mitigated, give mitigater contribution towards kill on contributer
    // I deal 1 damage buffed to 2 damage by ally, they mitigate 50%, they get 0.5 contribution to me and and ally
    // You = 0.5, Ally = 0.5, Mitigator = 1

    //[effect multi] Apply effect multi to outgoing contribution (it has no attribution)

    [ShowInInspector, ReadOnly, FoldoutGroup("Effect Modifiers")]
    public List<EffectModifier> effectModifiers = new List<EffectModifier>();
    private Dictionary<EffectTag, float> precalculatedModifiers = new Dictionary<EffectTag, float>();
    public void AddModifier(EffectTag tags, float magnitude, EffectModifierCalculationType type, float contributionMultiplier, Effect effect)
    {
        if(precalculatedModifiers.ContainsKey(tags))
            precalculatedModifiers.Remove(tags);
        switch (type)
        {
            case EffectModifierCalculationType.Flat:
                effectModifiers.Insert(0, new EffectModifier(tags, magnitude, type, contributionMultiplier, effect));
                break;
            case EffectModifierCalculationType.Additive:
            case EffectModifierCalculationType.Multiplicative:
                effectModifiers.Add(new EffectModifier(tags, magnitude, type, contributionMultiplier, effect));
                break;
        }
    }

    public void RemoveModifier(Effect effect)
    {
        effectModifiers.RemoveAll(x => x.effect == effect);
    }

    public float CalculateModifier(EffectTag tags = EffectTag.None)
    {
        float result = 0;
        if (precalculatedModifiers.ContainsKey(tags))
        {
            result = precalculatedModifiers[tags];
        }
        else
        {
            result = CalculateModifier(null, 0, 0, tags);
            precalculatedModifiers.Add(tags, result);
        }
        return result;
    }

    public float CalculateModifier(Effect contributingEffect, float contributionMultiplier, float mitigationContributionMultiplier, EffectTag additionalRequiredTags = EffectTag.None)
    {
        return CreateCalculation(null, contributingEffect, additionalRequiredTags).CalculateModifier(contributingEffect);
    }

    public EffectModifier CreateCalculation(EffectModifier rootCalculation, Effect contributingEffect, EffectTag additionalRequiredTags)
    {
        if (rootCalculation == null)
        {
            rootCalculation = new EffectModifier();
            if (contributingEffect == null)
            {
                rootCalculation.AddSubcalculation(additionalRequiredTags);
                rootCalculation.TryAddFlat(new EffectModifier(additionalRequiredTags, 1, EffectModifierCalculationType.Flat, 1, null));
                // Imaginary 1x effect multi
            }
            else
            {
                foreach (KeyValuePair<EffectTag, float> tag in contributingEffect.effectTags)
                {
                    rootCalculation.AddSubcalculation(tag.Key);
                    rootCalculation.TryAddFlat(new EffectModifier(tag.Key, tag.Value, EffectModifierCalculationType.Flat, 1, contributingEffect));
                }
                rootCalculation.submodifiers.Add(new EffectModifier(EffectTag.None, contributingEffect.effectMultiplier, EffectModifierCalculationType.Multiplicative, 0, contributingEffect));
            }
        }

        foreach (EffectModifier modifier in effectModifiers)
        {
            switch (modifier.type)
            {
                case EffectModifierCalculationType.Multiplicative:
                    {
                        rootCalculation.TryAddMultiplicative(modifier, additionalRequiredTags);
                        break;
                    }
                case EffectModifierCalculationType.Additive:
                    {
                        rootCalculation.TryAddAdditive(modifier, additionalRequiredTags);
                        break;
                    }
                case EffectModifierCalculationType.Flat:
                case EffectModifierCalculationType.FlatMultiplicative:
                    {
                        rootCalculation.TryAddFlat(modifier, additionalRequiredTags);
                        break;
                    }
            }
        }

        return rootCalculation;
    }

    public override void OnDestroy()
    {
        effectModifiers.Clear();
        precalculatedModifiers.Clear();
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
    Enmity        = 1 << 25,

    Global        = int.MaxValue
}

public enum EffectModifierCalculationType
{
    //                Stored As
    Flat, //          0 + 0.2
    FlatMultiplicative,
    Additive, //      1 + 0.2
    Multiplicative // 1 * 1.2
}

// Damage Type Needs
// DamageInstance must support multiple damage type values (Give it a dictionary<damagetype, float> with a default value)
// Each damage type value is buffed by different things during damage calculation ("subcalculations" / find modifiers per damagetype)
// Activate triggers per damage type (done in damageinstance)

// RF (Fire, Burning, DoT) <- affected by DoT targeting support gem (grants Duration tag)