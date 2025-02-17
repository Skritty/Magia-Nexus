using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PE_PreventExpire : PersistentEffect
{
    public Alignment affectedAlignment;
    public override void Activate()
    {
        tickDuration = (int)(tickDuration * effectMultiplier);
        base.Activate();
    }
    public override void OnTick()
    {
        foreach (PersistentEffect effect in Target.Stat<Stat_PersistentEffects>().persistentEffects)
        {
            if(effect.tick + 1 == effect.tickDuration && effect.alignment == alignment && effect is not PE_PreventExpire)
            {
                effect.tickDuration++;
            }
        }
    }
}
