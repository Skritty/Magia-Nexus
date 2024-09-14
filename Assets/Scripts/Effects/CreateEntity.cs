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
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget? target.transform.position : owner.transform.position, Quaternion.identity);

        spawnedEntity.Stat<Stat_Team>().team = owner.Stat<Stat_Team>().team;
        spawnedEntity.Stat<Stat_Target>().target = target;
        switch (entityType)
        {
            case EntityType.Basic:
                {
                    break;
                }
            case EntityType.Projectile:
                {
                    spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, owner.Stat<Stat_Movement>().facingDir);
                    spawnedEntity.Stat<Stat_Movement>().facingDir = (target.transform.position - owner.transform.position).normalized;
                    owner.Trigger<Trigger_ProjectileCreated>(spawnedEntity);
                    //original target, number of projectiles (done here), projectile speed,
                    //lifetime, damage modifiers, additional behavior
                    break;
                }
        }

        spawnedEntity.gameObject.SetActive(true);
    }
}
