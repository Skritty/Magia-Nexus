using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Projectile : GenericStat<Stat_Projectile>
{
    [FoldoutGroup("Projectile")]
    public int piercesRemaining;

    protected override void Initialize()
    {
        base.Initialize();
        Owner.Subscribe<Trigger_OnHit>(OnHit);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Owner.Unsubscribe<Trigger_OnHit>(OnHit);
    }

    private void OnHit(Trigger trigger)
    {
        DamageInstance damage = trigger.Data<DamageInstance>();
        if (--piercesRemaining < 0)
        {
            Owner.Trigger<Trigger_Expire>(damage.Owner);
        }
        else
        {
            Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_OnProjectilePierce>(damage, damage));
        }
    }
}