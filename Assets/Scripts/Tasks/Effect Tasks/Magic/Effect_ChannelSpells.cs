using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_ChannelSpells : EffectTask
{
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Spell spell in Target.GetStat<Mechanic_Magic>().ownedSpells)
        {
            spell.Stage++;
        }
    }
}
