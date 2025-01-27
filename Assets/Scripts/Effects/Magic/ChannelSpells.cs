using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelSpells : Effect
{
    public override void Activate()
    {
        foreach(Entity spell in Target.Stat<Stat_Magic>().ownedSpells)
        {
            spell.Stat<Stat_Magic>().Stage++;
        }
        
    }
}
