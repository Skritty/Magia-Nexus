using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum EffectModifierCalculationType
{
    //                Stored As
    Flat, //          0 + 0.2
    FlatMultiplicative,
    Additive, //      1 + 0.2
    Multiplicative // 1 * 1.2
}

[Serializable]
public class EffectModifier
{
    public EffectModifierCalculationType calculationType;
    public DamageType damageType;
    public EffectTag effectTag;
    public float magnitude;

    // For calculation
    [HideInInspector, NonSerialized]
    public Effect effect;
    [HideInInspector, NonSerialized]
    public List<EffectModifier> submodifiers = new List<EffectModifier>();

    // For calculating contribution
    [HideInInspector, NonSerialized]
    public float contributionMagnitude; // magnitude including contribution multiplier 100x * 0x
    [HideInInspector, NonSerialized]
    public float positive; // magnitude including contribution multiplier, without negative modifiers
    [HideInInspector, NonSerialized]
    public float negative;
    private void SetAllMagnitudes(float amount, float contributionMultiplier)
    {
        magnitude = amount;

        switch (calculationType)
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

    public EffectModifier() { }

    /// <summary>
    /// Create a new root EffectModifier
    /// </summary>
    public EffectModifier(bool root = false)
    {
        if (root)
        {
            SetAllMagnitudes(1, 1);
            calculationType = EffectModifierCalculationType.Multiplicative;
            EffectModifier subcalculations = new EffectModifier(EffectModifierCalculationType.Flat);
            //subcalculations.type = EffectModifierCalculationType.Additive;
            submodifiers.Add(subcalculations);
        }
    }

    protected EffectModifier(EffectModifierCalculationType type)
    {
        this.calculationType = type;
        if (type != EffectModifierCalculationType.Flat)
        {
            SetAllMagnitudes(1, 1);
        }
    }

    // Leaf node (contains a magnitude)
    public EffectModifier(DamageType damageType, EffectTag tag, float magnitude, EffectModifierCalculationType type, float contributionMultiplier, Effect effect)
    {
        this.calculationType = type;
        this.damageType = damageType;
        this.effectTag = tag;
        SetAllMagnitudes(magnitude, contributionMultiplier);
        this.effect = effect;
    }

    protected EffectModifier FindOrCreateSubcalculation(DamageType damageType)
    {
        foreach (EffectModifier subcalculation in submodifiers[0].submodifiers)
        {
            if (subcalculation.damageType.HasFlag(damageType))
            {
                return subcalculation;
            }
        }
        return AddSubcalculation(damageType);
    }

    /// <summary>
    /// Add a subcalculation to a root EffectModifier
    /// </summary>
    /// <param name="tags"></param>
    protected EffectModifier AddSubcalculation(DamageType damageType)
    {
        EffectModifier subcalculation = new EffectModifier(EffectModifierCalculationType.FlatMultiplicative);
        subcalculation.damageType = damageType;
        submodifiers[0].submodifiers.Add(subcalculation);
        // Flat
        subcalculation.submodifiers.Add(new EffectModifier(EffectModifierCalculationType.Flat));
        // Additive
        subcalculation.submodifiers.Add(new EffectModifier(EffectModifierCalculationType.Additive));
        return subcalculation;
    }

    public void AddFlat(EffectModifier modifier)
    {
        FindOrCreateSubcalculation(modifier.damageType).submodifiers[0].submodifiers.Add(modifier);
    }

    public void AddAdditive(EffectModifier modifier)
    {
        FindOrCreateSubcalculation(modifier.damageType).submodifiers[1].submodifiers.Add(modifier);
    }

    public void AddMultiplicative(EffectModifier modifier)
    {
        FindOrCreateSubcalculation(modifier.damageType).submodifiers.Add(modifier);
    }

    public static EffectModifier CreateCalculation(Effect contributingEffect, bool noEffectDefaultFlat = true, DamageType defaultFlat = DamageType.True)
    {
        EffectModifier rootCalculation = new EffectModifier(true);
        if (contributingEffect == null)
        {
            rootCalculation.AddSubcalculation(defaultFlat);
            if(noEffectDefaultFlat)
                rootCalculation.AddFlat(new EffectModifier(defaultFlat, EffectTag.None, 1, EffectModifierCalculationType.Flat, 1, null));
            // Imaginary 1x effect multi
        }
        else
        {
            rootCalculation.submodifiers.Add(new EffectModifier(DamageType.True, EffectTag.None, contributingEffect.effectMultiplier, EffectModifierCalculationType.Multiplicative, 0, contributingEffect));
        }
        return rootCalculation;
    }

    public EffectModifier AddModifier(EffectModifier modifier)
    {
        switch (modifier.calculationType)
        {
            case EffectModifierCalculationType.Multiplicative:
                {
                    AddMultiplicative(modifier);
                    break;
                }
            case EffectModifierCalculationType.Additive:
                {
                    AddAdditive(modifier);
                    break;
                }
            case EffectModifierCalculationType.Flat:
            case EffectModifierCalculationType.FlatMultiplicative:
                {
                    AddFlat(modifier); 
                    break;
                }
        }
        return this;
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
            switch (calculationType)
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

        if (submodifiers.Count > 0)
        {
            switch (calculationType)
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

    private void Contribute(Effect contributingEffect)
    {
        if (contributingEffect == null) return;

        Dictionary<Entity, float> contributors = new Dictionary<Entity, float>();
        CalculateContribution(contributors, contributionMagnitude, positive - contributionMagnitude);

        // Version 2
        float totalPositiveContribution = 0;
        foreach (KeyValuePair<Entity, float> contributor in contributors)
        {
            if (contributor.Value >= 0)
            {
                totalPositiveContribution += contributor.Value;
            }
        }

        // Hand out contributions
        foreach (KeyValuePair<Entity, float> contributor in contributors)
        {
            if (contributor.Value >= 0)
            {
                contributor.Key.GetMechanic<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Target, contributor.Value * contributingEffect.effectMultiplier);
            }
            else
            {
                // Version 1: All mitigated goes towards effect owner
                //contributor.Key.Stat<Stat_PlayerOwner>().ApplyContribution(contributingEffect.Owner, contributor.Value * contributingEffect.effectMultiplier);

                // Version 2: Mitigated is split between contributors to effect (not just effect owner, but buff owners too)
                foreach (KeyValuePair<Entity, float> contributor2 in contributors)
                {
                    if (contributor2.Value <= 0) continue;
                    contributor.Key.GetMechanic<Stat_PlayerOwner>().ApplyContribution(contributor2.Key, -contributor.Value * contributor2.Value / totalPositiveContribution * contributingEffect.effectMultiplier);
                }
            }
        }
    }
    private void CalculateContribution(Dictionary<Entity, float> contributors, float totalMagnitude, float mitigatedMagnitude)
    {
        // Is leaf
        if (effect != null && effect.Owner)
        {
            if (magnitude > 0)
            {
                if (float.IsNaN(totalMagnitude))
                {
                    // TODO: this keeps happening
                    //Debug.LogWarning($"NAN Contribution Warning! Owner: {effect.Owner}, Effect: {effect.Source}, Magnitude: {magnitude}, TotalMagnitude: {totalMagnitude}");
                }
                else if (!contributors.TryAdd(effect.Owner, totalMagnitude))
                {
                    contributors[effect.Owner] += totalMagnitude;
                }
            }
            else //if(effect.Owner.Stat<Stat_Team>().team != contributingEffect.Owner.Stat<Stat_Team>().team)
            {
                if (float.IsNaN(totalMagnitude))
                {
                    //Debug.LogWarning($"NAN Contribution Warning! Owner: {effect.Owner}, Effect: {effect.Source}, Magnitude: {magnitude}");
                }
                else if (!contributors.TryAdd(effect.Owner, -mitigatedMagnitude))
                {
                    contributors[effect.Owner] -= mitigatedMagnitude;
                }
            }
        }

        float sum = 0;
        float negativeSum = 0;
        foreach (EffectModifier modifier in submodifiers)
        {
            switch (modifier.calculationType)
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
            switch (modifier.calculationType)
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

    public EffectModifier Clone()
    {
        return (EffectModifier)MemberwiseClone();
    }
}

// Damage calculation 
// Root: ((damage type subcalculation) + ...)
// Damage type subcalculation: ((Flat + ...) * (Increased + ...) * Multiplier * ...)

//                                                      (ROOT REAL)
// n0 Multiplicative: --------------------------(root [flat])------------------------------ * (effect multiplier [increased])
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



// Damage Type Needs
// DamageInstance must support multiple damage type values (Give it a dictionary<damagetype, float> with a default value)
// Each damage type value is buffed by different things during damage calculation ("subcalculations" / find modifiers per damagetype)
// Activate triggers per damage type (done in damageinstance)

// RF (Fire, Burning, DoT) <- affected by DoT targeting support gem (grants Duration tag)