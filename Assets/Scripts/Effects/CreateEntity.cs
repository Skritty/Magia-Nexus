using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CreateEntity : Effect
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
                    Create(0).GetMechanic<Stat_Actions>().Tick();
                    break;
                }
            case EntityType.Projectile:
                {
                    int projectileCount = numberOfProjectiles + (ignoreAdditionalProjectiles ? 0 : (int)Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.Projectiles));
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Entity spawnedEntity = Create(i);
                        projectileFanAngle = Mathf.Clamp(projectileFanAngle, 0f, 180f);
                        if (projectileCount > 1)
                            switch (projectileFanType)
                            {
                                case ProjectileFanType.EvenlySpaced:
                                    {
                                        spawnedEntity.GetMechanic<Stat_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            i * 1f / (projectileCount - 1)))
                                            * spawnedEntity.GetMechanic<Stat_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Shotgun:
                                    {
                                        spawnedEntity.GetMechanic<Stat_Movement>().facingDir =
                                            Quaternion.Euler(0, 0,
                                            Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                            UnityEngine.Random.Range(0f, 1f)))
                                            * spawnedEntity.GetMechanic<Stat_Movement>().facingDir;
                                        break;
                                    }
                                case ProjectileFanType.Sequence:
                                    {
                                        int tickDelay = (int)(1f * Owner.GetMechanic<Stat_Actions>().TicksPerAction / projectileCount);
                                        for (int tick = 0; tick < tickDelay; tick++)
                                        {
                                            yield return new WaitForFixedUpdate();
                                        }
                                        break;
                                    }
                            }
                        spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Stat_Movement>().facingDir);
                        new Trigger_ProjectileCreated(spawnedEntity, spawnedEntity, entity, this, Source);
                        spawnedEntity.GetMechanic<Stat_Actions>().Tick();
                    }
                    break;
                }
            case EntityType.Summon:
                {
                    int summonCount = numberOfSummons + (ignoreAdditionalSummons ? 0 : (int)Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.Summons));
                    int maxSummons = overcapSummons + (int)Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.MaxSummons);
                    for (int i = 0; i < summonCount; i++)
                    {
                        if (Owner.GetMechanic<Stat_Team>().summons.Count >= maxSummons) break;
                        Entity spawnedEntity = Create(i);
                        spawnedEntity.Stat<Stat_MaxLife>().Value *= lifeMultiplier;
                        spawnedEntity.Stat<Stat_CurrentLife>().Value *= lifeMultiplier;
                        // TODO: summon position
                        Owner.GetMechanic<Stat_Team>().summons.Add(spawnedEntity);
                        Trigger_Expire.Subscribe(x => Owner.GetMechanic<Stat_Team>().summons.Remove(spawnedEntity), spawnedEntity);
                        new Trigger_SummonCreated(spawnedEntity, spawnedEntity, entity, this, Source);
                        spawnedEntity.GetMechanic<Stat_Actions>().Tick();
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
        spawnedEntity.GetMechanic<Stat_PlayerOwner>().SetPlayer(Owner.GetMechanic<Stat_PlayerOwner>());
        spawnedEntity.GetMechanic<Stat_PlayerOwner>().proxyOwner = Owner;
        spawnedEntity.GetMechanic<Stat_Team>().team = Owner.GetMechanic<Stat_Team>().team;
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                spawnedEntity.GetMechanic<Stat_Movement>().movementTarget = Owner;
                break;
            case EffectTargetSelector.Target:
                spawnedEntity.GetMechanic<Stat_Movement>().movementTarget = Target;
                break;
        }
        spawnedEntity.GetMechanic<Stat_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
