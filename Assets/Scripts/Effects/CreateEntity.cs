using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateEntity : Effect
{
    public enum EntityType
    {
        Basic = 1,
        Projectile = 2
    }

    public enum ProjectileFanType
    {
        EvenlySpaced = 1,
        Sequence = 2,
        Shotgun = 4,
    }

    public EntityType entityType = EntityType.Basic;
    public Entity entity;
    public bool spawnOnTarget;
    public ProjectileFanType projectileFanType = ProjectileFanType.Sequence;
    public float projectileFanAngle = 45;

    public override void Activate()
    {
        switch (entityType)
        {
            case EntityType.Basic:
                {
                    Create();
                    break;
                }
            case EntityType.Projectile:
                {
                    for(int i = 0; i < Owner.Stat<Stat_EffectModifiers>().projectilesFired; i++)
                    {
                        Entity spawnedEntity = Create();
                        switch (projectileFanType)
                        {
                            case ProjectileFanType.EvenlySpaced:
                                {
                                    spawnedEntity.Stat<Stat_Movement>().facingDir = 
                                        Quaternion.Euler(0, 
                                        Mathf.Lerp(-projectileFanAngle, projectileFanAngle, 
                                        i * 1f / (Owner.Stat<Stat_EffectModifiers>().projectilesFired-1)), 0) 
                                        * spawnedEntity.Stat<Stat_Movement>().facingDir;
                                    break;
                                }
                            case ProjectileFanType.Shotgun:
                                {
                                    spawnedEntity.Stat<Stat_Movement>().facingDir =
                                        Quaternion.Euler(0,
                                        Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                        UnityEngine.Random.Range(0f, 1f)), 0)
                                        * spawnedEntity.Stat<Stat_Movement>().facingDir;
                                    break;
                                }
                        }
                        spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
                        Owner.Trigger<Trigger_ProjectileCreated>(spawnedEntity);
                    }
                    break;
                }
        }
    }

    private Entity Create()
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);

        spawnedEntity.Stat<Stat_PlayerOwner>().SetPlayer(Owner.Stat<Stat_PlayerOwner>());
        spawnedEntity.Stat<Stat_Team>().team = Owner.Stat<Stat_Team>().team;
        spawnedEntity.Stat<Stat_Movement>().movementTarget = Target;
        spawnedEntity.Stat<Stat_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
