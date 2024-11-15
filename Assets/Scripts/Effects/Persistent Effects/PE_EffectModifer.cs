using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    [InfoBox("Individual effect tag requires AND. Each effect tag is an OR. Flat modifiers add all tag keys together.")]
    [FoldoutGroup("@GetType()")]
    public EffectModifierCalculationType modifierType;
    [FoldoutGroup("@GetType()")]
    public bool scaleWithEffectMulti;
    public override void OnGained()
    {
        Target.Stat<Stat_EffectModifiers>().AddModifier(this, modifierType, (scaleWithEffectMulti ? effectMultiplier : 1));
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
    }
}