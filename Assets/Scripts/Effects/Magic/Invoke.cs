using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TwitchLib.Api.Helix;
using UnityEngine;

public class Invoke : Effect
{
    public bool useOwnerRunes;
    public List<Rune> runes = new List<Rune>();
    public override void Activate()
    {
        Spell spell;
        if (useOwnerRunes)
        {
            if (Owner.GetMechanic<Stat_Magic>().runes.Count == 0) return;
            spell = new Spell(Owner, Owner.GetMechanic<Stat_Magic>().runes);
        }
        else
        {
            if (runes.Count == 0) return;
            spell = new Spell(Owner, runes);
        }
        spell.GenerateSpell(this, null);
        Owner.GetMechanic<Stat_Magic>().ConsumeRunes();
    }
}