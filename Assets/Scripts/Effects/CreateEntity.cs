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
                    Create();
                    break;
                }
            case EntityType.Projectile:
                {
                    int numberOfProjectiles = (int)Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.Projectile, EffectTag.Targets);
                    for (int i = 0; i < numberOfProjectiles; i++)
                    {
                        Entity spawnedEntity = Create();
                        if(numberOfProjectiles > 1)
                            switch (projectileFanType)
                            {
                                case ProjectileFanType.EvenlySpaced:
                                    {
                                        spawnedEntity.Stat<Stat_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            i * 1f / (numberOfProjectiles - 1)))
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
                                        int tickDelay = (int)(1f * Owner.Stat<Stat_Actions>().ticksPerPhase / numberOfProjectiles);
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
