using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    [InfoBox("Formatting\nFlat: 0.2 = (+0.2 flat) | Increased: 0.2 = (20% increased) | More: 1.2 = (20% more)")]
    [FoldoutGroup("@GetType()")]
    public EffectModifierCalculationType modifierType;
    [FoldoutGroup("@GetType()")]
    public float contributionMultiplier = 1;
    [FoldoutGroup("@GetType()")]
    public bool scaleWithEffectMulti;
    public override void OnGained()
    {
        foreach (KeyValuePair<EffectTag, float> effectTag in effectTags)
        {
            Target.Stat<Stat_EffectModifiers>().AddModifier(effectTag.Key, effectTag.Value * (scaleWithEffectMulti ? effectMultiplier : 1), modifierType, contributionMultiplier, this);
        }
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
    }
}