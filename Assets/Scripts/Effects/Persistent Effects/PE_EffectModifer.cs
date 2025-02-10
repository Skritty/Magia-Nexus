using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using Unity.VisualScripting;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{

    [InfoBox("Formatting\nFlat: 0.2 = (+0.2 flat) | Increased: 0.2 = (20% increased) | More: 1.2 = (20% more)")]
    [FoldoutGroup("@GetType()")]
    public List<EffectModifier> effectModifiers = new List<EffectModifier>();
    public override void OnGained()
    {
        foreach (EffectModifier modifier in effectModifiers)
        {
            EffectModifier newMod = modifier.Clone();
            newMod.effect = this;
            Target.Stat<Stat_EffectModifiers>().AddModifier(newMod);
        }
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
    }
}