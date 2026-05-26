using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_CreateEntity : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public Entity entity;
    [FoldoutGroup("@GetType()")]
    public bool spawnOnTarget;
    [FoldoutGroup("@GetType()")]
    public EffectTargetSelector movementTarget = EffectTargetSelector.Target;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Owner.StartCoroutine(SpawnEntity(Owner, Target, multiplier, triggered));
    }

    protected virtual IEnumerator SpawnEntity(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Create(Owner, Target, multiplier, triggered, 0).GetStat<Mechanic_Skills>().Tick();
        yield return null;
    }

    protected Entity Create(Entity Owner, Entity Target, float multiplier, bool triggered, int id)
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);
        spawnedEntity.gameObject.name = spawnedEntity.gameObject.name + id;
        spawnedEntity.GetStat<Stat_PlayerCharacter>().Value = Owner;

        spawnedEntity.GetStat<Stat_Faction>().Add(Owner.GetStat<Stat_Faction>());
        //spawnedEntity.GetStat<Stat_DamageDealt>().Add(Owner.GetStat<Stat_DamageDealt>());
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                Stats.GetStat<Stat_Targets>(spawnedEntity).Add(Owner);
                break;
            case EffectTargetSelector.Target:
                Stats.GetStat<Stat_Targets>(spawnedEntity).Add(Target);
                break;
        }
        spawnedEntity.GetStat<Mechanic_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
