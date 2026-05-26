using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_Summon : Effect_CreateEntity
{
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

    protected override IEnumerator SpawnEntity(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        int summonCount = numberOfSummons + (ignoreAdditionalSummons ? 0 : (int)Stats.GetStat<Stat_SummonCount>(Owner).Value);
        int maxSummons = overcapSummons + (int)Stats.GetStat<Stat_MaxSummons>(Owner).Value;
        for (int i = 0; i < summonCount; i++)
        {
            if (Stats.GetStat<Stat_Summons>(Owner).Count >= maxSummons) break;
            Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
            spawnedEntity.GetStat<Stat_MaxLife>().Add(new NumericalSolver(lifeMultiplier, CalculationStep.Multiplicative));
            // TODO: summon position
            Stats.GetStat<Stat_Summons>(Owner).Add(spawnedEntity);
            Trigger_Expire.Subscribe(x => Stats.GetStat<Stat_Summons>(Owner).Remove(spawnedEntity), spawnedEntity);
            Trigger_SummonCreated.Invoke(spawnedEntity, spawnedEntity, entity, this);
            spawnedEntity.GetStat<Mechanic_Skills>().Tick();
        }
        yield return null;
    }
}
