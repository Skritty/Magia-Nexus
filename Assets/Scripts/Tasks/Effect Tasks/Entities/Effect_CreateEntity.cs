using System;
using System.Collections;
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
        Create(Owner, Target, multiplier, triggered, 0).GetMechanic<Mechanic_Actions>().Tick();
        yield return null;
    }

    protected Entity Create(Entity Owner, Entity Target, float multiplier, bool triggered, int id)
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);
        spawnedEntity.gameObject.name = spawnedEntity.gameObject.name + id;
        spawnedEntity.GetMechanic<Mechanic_PlayerOwner>().SetPlayer(Owner.GetMechanic<Mechanic_PlayerOwner>());
        spawnedEntity.GetMechanic<Mechanic_PlayerOwner>().proxyOwner = Owner;
        spawnedEntity.Stat<Stat_Team>().Add(Owner.Stat<Stat_Team>().Value);
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                spawnedEntity.Stat<Stat_MovementTarget>().Add(Owner);
                break;
            case EffectTargetSelector.Target:
                spawnedEntity.Stat<Stat_MovementTarget>().Add(Target);
                break;
        }
        spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
