using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelSpells : Effect
{
    public override void Activate()
    {
        foreach(Spell spell in Target.GetMechanic<Stat_Magic>().ownedSpells)
        {
            spell.Stage++;
        }
        
    }
}
