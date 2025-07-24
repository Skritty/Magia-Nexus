using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_ChannelSpells<T> : EffectTask<T>
{
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Spell spell in Target.GetMechanic<Mechanic_Magic>().ownedSpells)
        {
            spell.Stage++;
        }
    }
}
