using Sirenix.OdinInspector;
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

    [FoldoutGroup("@GetType()")]
    public EntityType entityType = EntityType.Basic;
    [FoldoutGroup("@GetType()")]
    public Entity entity;
    [FoldoutGroup("@GetType()")]
    public bool spawnOnTarget;
    [FoldoutGroup("@GetType()")]
    public MovementTarget movementTarget = MovementTarget.Target;
    [FoldoutGroup("Projectile Behavior")]
    public int numberOfProjectiles = 1;
    [FoldoutGroup("Projectile Behavior")]
    public bool ignoreAdditionalProjectiles;
    [FoldoutGroup("Projectile Behavior")]
    public ProjectileFanType projectileFanType = ProjectileFanType.Sequence;
    [FoldoutGroup("Projectile Behavior")]
    public float projectileFanAngle = 45;

    public override void Activate()
    {
        Owner.StartCoroutine(SpawnEntity());
    }

    private IEnumerator SpawnEntity()
    {
        switch (entityType)
        {
            case EntityType.Basic:
                {
                    Create(0);
                    break;
                }
            case EntityType.Projectile:
                {
                    int projectileCount = numberOfProjectiles + (ignoreAdditionalProjectiles ? 0 : (int)Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.Projectile | EffectTag.Targets) - 1);
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Entity spawnedEntity = Create(i);
                        projectileFanAngle = Mathf.Clamp(projectileFanAngle, 0f, 180f);
                        if (projectileCount > 1)
                            switch (projectileFanType)
                            {
                                case ProjectileFanType.EvenlySpaced:
                                    {
                                        spawnedEntity.Stat<Stat_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            i * 1f / (projectileCount - 1)))
                                            * spawnedEntity.Stat<Stat_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Shotgun:
                                    {
                                        spawnedEntity.Stat<Stat_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            UnityEngine.Random.Range(0f, 1f)))
                                            * spawnedEntity.Stat<Stat_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Sequence:
                                    {
                                        int tickDelay = (int)(1f * Owner.Stat<Stat_Actions>().ticksPerPhase / projectileCount);
                                        for (int tick = 0; tick < tickDelay; tick++)
                                        {
                                            yield return new WaitForFixedUpdate();
                                        }
                                        break;
                                    }
                            }
                        spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
                        Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_ProjectileCreated>(spawnedEntity, this));
                    }
                    break;
                }
        }
    }

    private Entity Create(int id)
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);
        spawnedEntity.gameObject.name = ""+id;
        spawnedEntity.Stat<Stat_PlayerOwner>().SetPlayer(Owner.Stat<Stat_PlayerOwner>());
        spawnedEntity.Stat<Stat_Team>().team = Owner.Stat<Stat_Team>().team;
        switch (movementTarget)
        {
            case MovementTarget.Owner:
                spawnedEntity.Stat<Stat_Movement>().movementTarget = Owner;
                break;
            case MovementTarget.Target:
                spawnedEntity.Stat<Stat_Movement>().movementTarget = Target;
                break;
        }
        spawnedEntity.Stat<Stat_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
