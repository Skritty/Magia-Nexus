using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_EffectModifiers : Mechanic<Stat_EffectModifiers>
{
    [SerializeReference, FoldoutGroup("Effect Modifiers")]
    private List<NumericalModifier> effectModifiers = new List<NumericalModifier>();
    private Dictionary<EffectTag, dynamic> precalculatedModifiers = new Dictionary<EffectTag, dynamic>();

    public void AddModifier(NumericalModifier modifier)
    {
        /*if (precalculatedModifiers.ContainsKey(modifier.tag))
            precalculatedModifiers.Remove(modifier.tag);
        effectModifiers.Add(modifier);
        effectModifiers.Sort((x, y) =>
        {
            switch (x.method)
            {
                case NumericalModifierCalculationMethod.Flat:
                    return -1;
                case NumericalModifierCalculationMethod.Additive:
                case NumericalModifierCalculationMethod.Multiplicative:
                    return 0;
            }
            return 0;
        });*/
    }

    public void RemoveModifier(Effect effect)
    {
        /*foreach(NumericalModifier modifier in effectModifiers.ToArray())
        {
            if (modifier.source != effect) continue;
            if (precalculatedModifiers.ContainsKey(modifier.tag))
                precalculatedModifiers.Remove(modifier.tag);
            effectModifiers.Remove(modifier);
        }*/
    }

    /// <summary>
    /// Adds valid effect modifiers to the calculation
    /// </summary>
    public void AddModifiersToCalculation(NumericalModifier calculation, EffectTag tag)
    {
        /*foreach (NumericalModifier modifier in effectModifiers)
        {
            if (modifier.tag != tag) continue;
            calculation.AddModifier(new AssistContributingModifier(modifier, 1), modifier.method);
        }*/
    }

    public float CalculateModifier(EffectTag tag)
    {
        return 0;
        /*if (!precalculatedModifiers.ContainsKey(tag))
        {
            NumericalModifier calculation = NumericalModifier.CreateCalculation();
            AddModifiersToCalculation(calculation, tag);
            if (Owner.Stat<Stat_PlayerOwner>().scaleWithPlayerCharacterModifiers && !Owner.Stat<Stat_PlayerOwner>().playerCharacter)
            {
                Owner.Stat<Stat_PlayerOwner>().playerEntity.Stat<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, tag);
            }
            precalculatedModifiers.Add(tag, calculation.Solve());
        }
        return precalculatedModifiers[tag];*/
    }

    public override void OnDestroy()
    {
        effectModifiers.Clear();
        precalculatedModifiers.Clear();
    }
}