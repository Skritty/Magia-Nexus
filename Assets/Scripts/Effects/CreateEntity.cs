using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateEntity : Effect
{
    [Flags]
    public enum EntityType
    {
        Basic = 1,
        Projectile = 2
    }

    public EntityType entityType;
    public Entity entity;
    public bool spawnOnTarget;

    public override void Activate()
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);

        spawnedEntity.Stat<Stat_PlayerOwner>().SetPlayer(Owner.Stat<Stat_PlayerOwner>());
        spawnedEntity.Stat<Stat_Team>().team = Owner.Stat<Stat_Team>().team;
        spawnedEntity.Stat<Stat_Movement>().movementTarget = Target;
        spawnedEntity.Stat<Stat_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        switch (entityType)
        {
            case EntityType.Basic:
                {
                    break;
                }
            case EntityType.Projectile:
                {
                    spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
                    Owner.Trigger<Trigger_ProjectileCreated>(spawnedEntity);
                    //original target, number of projectiles (done here), projectile speed,
                    //lifetime, damage modifiers, additional behavior
                    break;
                }
        }
    }
}
