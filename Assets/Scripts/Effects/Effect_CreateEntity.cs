using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Effect_CreateEntity : EffectTask // TODO: split into multiple child classes
{
    public enum EntityType { Basic, Projectile, Summon }
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
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;

    [FoldoutGroup("Projectile Behavior")]
    public int numberOfProjectiles = 1;
    [FoldoutGroup("Projectile Behavior")]
    public bool ignoreAdditionalProjectiles;
    [FoldoutGroup("Projectile Behavior")]
    public ProjectileFanType projectileFanType = ProjectileFanType.Sequence;
    [FoldoutGroup("Projectile Behavior"), Range(0f, 180f)]
    public float projectileFanAngle = 45;

    [FoldoutGroup("Summon Behavior")]
    public int numberOfSummons = 1;
    [FoldoutGroup("Summon Behavior")]
    public int overcapSummons = 0;
    [FoldoutGroup("Summon Behavior")]
    public float damageMultiplier = 1;
    [FoldoutGroup("Summon Behavior")]
    public float lifeMultiplier = 1;
    [FoldoutGroup("Summon Behavior")]
    public bool ignoreAdditionalSummons;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Owner.StartCoroutine(SpawnEntity(Owner, Target, multiplier, triggered));
    }

    private IEnumerator SpawnEntity(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        switch (entityType)
        {
            case EntityType.Basic:
                {
                    Create(Owner, Target, multiplier, triggered, 0).GetMechanic<Mechanic_Actions>().Tick();
                    break;
                }
            case EntityType.Projectile:
                {
                    int projectileCount = numberOfProjectiles + (ignoreAdditionalProjectiles ? 0 : (int)Owner.Stat<Stat_Projectiles>().Value);
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
                        projectileFanAngle = Mathf.Clamp(projectileFanAngle, 0f, 180f);
                        if (projectileCount > 1)
                            switch (projectileFanType)
                            {
                                case ProjectileFanType.EvenlySpaced:
                                    {
                                        spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            i * 1f / (projectileCount - 1)))
                                            * spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Shotgun:
                                    {
                                        spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            UnityEngine.Random.Range(0f, 1f)))
                                            * spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Sequence:
                                    {
                                        int tickDelay = (int)(1f * Owner.GetMechanic<Mechanic_Actions>().TicksPerAction / projectileCount);
                                        for (int tick = 0; tick < tickDelay; tick++)
                                        {
                                            yield return new WaitForFixedUpdate();
                                        }
                                        break;
                                    }
                            }
                        spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Mechanic_Movement>().facingDir);
                        new Trigger_ProjectileCreated(spawnedEntity, spawnedEntity, entity, this);
                        spawnedEntity.GetMechanic<Mechanic_Actions>().Tick();
                    }
                    break;
                }
            case EntityType.Summon:
                {
                    int summonCount = numberOfSummons + (ignoreAdditionalSummons ? 0 : (int)Owner.Stat<Stat_SummonCount>().Value);
                    int maxSummons = overcapSummons + (int)Owner.Stat<Stat_MaxSummons>().Value;
                    for (int i = 0; i < summonCount; i++)
                    {
                        if (Owner.Stat<Stat_Summons>().Modifiers.Count >= maxSummons) break;
                        Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
                        spawnedEntity.Stat<Stat_MaxLife>().Value *= lifeMultiplier;
                        spawnedEntity.Stat<Stat_CurrentLife>().Value *= lifeMultiplier;
                        // TODO: summon position
                        Owner.Stat<Stat_Summons>().Value.Add(spawnedEntity);
                        Trigger_Expire.Subscribe(x => Owner.Stat<Stat_Summons>().Value.Remove(spawnedEntity), spawnedEntity);
                        new Trigger_SummonCreated(spawnedEntity, spawnedEntity, entity, this);
                        spawnedEntity.GetMechanic<Mechanic_Actions>().Tick();
                    }
                    break;
                }
        }
    }

    private Entity Create(Entity Owner, Entity Target, float multiplier, bool triggered, int id)
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);
        spawnedEntity.gameObject.name = ""+id;
        spawnedEntity.GetMechanic<Mechanic_PlayerOwner>().SetPlayer(Owner.GetMechanic<Mechanic_PlayerOwner>());
        spawnedEntity.GetMechanic<Mechanic_PlayerOwner>().proxyOwner = Owner;
        spawnedEntity.Stat<Stat_Team>().Value = Owner.Stat<Stat_Team>().Value;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                spawnedEntity.GetMechanic<Mechanic_Movement>().movementTarget = Owner;
                break;
            case EffectTargetSelector.Target:
                spawnedEntity.GetMechanic<Mechanic_Movement>().movementTarget = Target;
                break;
        }
        spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
