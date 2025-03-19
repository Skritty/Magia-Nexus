using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TwitchLib.Api.Helix;
using UnityEngine;

public class Invoke : Effect
{
    public override void Activate()
    {
        if (Owner.Stat<Stat_Magic>().runes.Count == 0) return;
        Spell spell = new Spell(Owner, Owner.Stat<Stat_Magic>().runes);
        spell.GenerateSpell(this, null);
        Owner.Stat<Stat_Magic>().ConsumeRunes();
    }
}