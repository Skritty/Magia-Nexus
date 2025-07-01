using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Common;
using Unity.VisualScripting;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    // TODO: make one of these for numerical modifiers (not just damage mods)
    [InfoBox("Formatting\nFlat: 0.2 = (+0.2 flat) | Increased: 0.2 = (20% increased) | More: 1.2 = (20% more)")]
    [FoldoutGroup("@GetType()")]
    public List<DamageModifier> effectModifiers = new List<DamageModifier>();
    public override void OnGained()
    {
        foreach (DamageModifier modifier in effectModifiers)
        {
            DamageModifier newMod = modifier.Clone();
            newMod.source = this;
            Target.GetMechanic<Stat_EffectModifiers>().AddModifier(newMod);
        }
    }

    public override void OnLost()
    {
        Target.GetMechanic<Stat_EffectModifiers>().RemoveModifier(this);
    }
}