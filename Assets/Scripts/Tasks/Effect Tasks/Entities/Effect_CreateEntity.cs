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
        Create(Owner, Target, multiplier, triggered, 0).GetStat<Mechanic_Skills>().Tick();
        yield return null;
    }

    protected Entity Create(Entity Owner, Entity Target, float multiplier, bool triggered, int id)
    {
        Entity spawnedEntity = GameObject.Instantiate(entity, spawnOnTarget ? Target.transform.position : Owner.transform.position, Quaternion.identity);
        spawnedEntity.gameObject.SetActive(true);
        spawnedEntity.gameObject.name = spawnedEntity.gameObject.name + id;
        spawnedEntity.GetStat<Stat_PlayerCharacter>().Value = Owner;
        spawnedEntity.GetStat<Stat_Viewer>().Value = Stats.GetStat<Stat_Viewer>(Owner).Value;
        Stats.GetStat<Stat_Team>(spawnedEntity).Add(Stats.GetStat<Stat_Team>(Owner).Value);
        switch (movementTarget)
        {
            case EffectTargetSelector.Owner:
                Stats.GetStat<Stat_MovementTarget>(spawnedEntity).Add(Owner);
                break;
            case EffectTargetSelector.Target:
                Stats.GetStat<Stat_MovementTarget>(spawnedEntity).Add(Target);
                break;
        }
        spawnedEntity.GetStat<Mechanic_Movement>().facingDir = (Target.transform.position - Owner.transform.position).normalized;

        return spawnedEntity;
    }
}
