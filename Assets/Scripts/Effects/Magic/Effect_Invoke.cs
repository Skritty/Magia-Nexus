using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TwitchLib.Api.Helix;
using UnityEngine;

public class Effect_Invoke : EffectTask
{
    public bool useOwnerRunes;
    public List<Rune> runes = new List<Rune>();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Spell spell;
        if (useOwnerRunes)
        {
            if (Owner.GetMechanic<Mechanic_Magic>().runes.Count == 0) return;
            spell = new Spell(Owner, Owner.GetMechanic<Mechanic_Magic>().runes);
        }
        else
        {
            if (runes.Count == 0) return;
            spell = new Spell(Owner, runes);
        }
        spell.GenerateSpell(this, null);
        Owner.GetMechanic<Mechanic_Magic>().ConsumeRunes();
    }
}