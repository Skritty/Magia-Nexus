using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_ProjectileOnHit : Effect
{
    public override void Activate(Trigger trigger)
    {
        DamageInstance damage = trigger.Data<DamageInstance>();
        if(damage == null)
        {
            Debug.LogError("Projectile On Hit can only be used with damage triggers");
        }
        if (--damage.owner.Stat<Stat_Projectile>().piercesRemaining < 0)
        {
            damage.owner.Trigger<Trigger_Expire>(damage.owner);
        }
        else
        {
            damage.owner.Trigger<Trigger_OnProjectilePierce>(damage);
        }
    }

    public override void Activate()
    {
        Debug.LogError("Projectile On Hit can only be used as a triggered effect");
    }
}