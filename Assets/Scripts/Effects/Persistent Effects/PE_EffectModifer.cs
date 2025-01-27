using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    protected override bool UsedInCalculations => true;

    [InfoBox("Formatting\nFlat: 0.2 = (+0.2 flat) | Increased: 0.2 = (20% increased) | More: 1.2 = (20% more)")]
    [FoldoutGroup("@GetType()")]
    public EffectModifierCalculationType modifierType;
    [FoldoutGroup("@GetType()")]
    public float contributionMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public bool scaleWithEffectMulti;
    [FoldoutGroup("@GetType()")]
    public bool multiplyByTicks;
    public override void OnGained()
    {
        if (!multiplyByTicks)
        {
            foreach (KeyValuePair<EffectTag, float> effectTag in effectTags)
            {
                Target.Stat<Stat_EffectModifiers>().AddModifier(effectTag.Key, effectTag.Value * (scaleWithEffectMulti ? effectMultiplier : 1), modifierType, contributionMultiplier, this);
            }
        }
    }

    public override void OnTick()
    {
        if (multiplyByTicks)
        {
            Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
            foreach (KeyValuePair<EffectTag, float> effectTag in effectTags)
            {
                Target.Stat<Stat_EffectModifiers>().AddModifier(effectTag.Key, effectTag.Value * tick * (scaleWithEffectMulti ? effectMultiplier : 1), modifierType, contributionMultiplier, this);
            }
        }
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
    }
}