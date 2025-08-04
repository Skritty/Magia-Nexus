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
        int summonCount = numberOfSummons + (ignoreAdditionalSummons ? 0 : (int)Owner.Stat<Stat_SummonCount>().Value);
        int maxSummons = overcapSummons + (int)Owner.Stat<Stat_MaxSummons>().Value;
        for (int i = 0; i < summonCount; i++)
        {
            if (Owner.Stat<Stat_Summons>().Count >= maxSummons) break;
            Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
            spawnedEntity.Stat<Stat_MaxLife>().Add(new NumericalSolver(lifeMultiplier, CalculationStep.Multiplicative));
            // TODO: summon position
            Owner.Stat<Stat_Summons>().Add(spawnedEntity);
            Trigger_Expire.Subscribe(x => Owner.Stat<Stat_Summons>().Remove(spawnedEntity), spawnedEntity);
            Trigger_SummonCreated.Invoke(spawnedEntity, spawnedEntity, entity, this);
            spawnedEntity.GetMechanic<Mechanic_Actions>().Tick();
        }
        yield return null;
    }
}
