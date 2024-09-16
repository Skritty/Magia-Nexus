using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Projectile : GenericStat<Stat_Projectile>
{
    [FoldoutGroup("Projectile")]
    public int numberOfProjectiles = 1;
    [FoldoutGroup("Projectile")]
    public int piercesRemaining;

    protected override void Initialize()
    {
        base.Initialize();
        owner.Subscribe<Trigger_OnHit>(OnHit);
    }

    private void OnHit(Trigger trigger)
    {
        DamageInstance damage = trigger.Data<DamageInstance>();
        if (--piercesRemaining < 0)
        {
            owner.Trigger<Trigger_Expire>(damage.Owner);
        }
        else
        {
            owner.Trigger<Trigger_OnProjectilePierce>(damage);
        }
    }
}