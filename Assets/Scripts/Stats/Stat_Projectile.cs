using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Projectile : GenericStat<Stat_Projectile>
{
    [FoldoutGroup("Projectile")]
    public int piercesRemaining;
    [FoldoutGroup("Projectile")]
    public int splitsRemaining;

    protected override void Initialize()
    {
        base.Initialize();
        Trigger_OnHit.Subscribe(OnHit);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnHit(Trigger_OnHit trigger)
    {
        if(--splitsRemaining >= 0)
        {
            CreateEntity split = new CreateEntity();
            split.ignoreAdditionalProjectiles = true;
            split.entity = Owner;
            split.Create(Owner); // TODO: Split from target (pseudo-owner?)
        }
        if (--piercesRemaining < 0)
        {
            new Trigger_Expire(trigger.damage.Owner);
        }
        else
        {
            new Trigger_OnProjectilePierce(trigger.damage);
        }
    }
}