using System.Collections.Generic;
using UnityEngine;

public class Stat_CenterEntity : Stat<Entity>, IStat<Entity> { }
public class Targeting_CenterOfGroup : MultiTargeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        if (owner == null) return new List<Entity>();

        List<Entity> targets = new List<Entity>();
        TargetFilter targetType;
        bool firstTarget = true;
        foreach (Entity entity in Entity.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) // TODO: Don't use find objects by type
        {
            // Can it be targeted?
            if (entity.Stat<Stat_Untargetable>().Contains((owner, this))) continue;

            // Is it in the affected entities?
            if (entity.Stat<Stat_Team>().Value != owner.Stat<Stat_Team>().Value)
            {
                targetType = TargetFilter.Enemies;
            }
            else if (entity == owner)
            {
                targetType = TargetFilter.Self;
            }
            else if (entity == owner.Stat<Stat_PlayerCharacter>().Value)
            {
                targetType = TargetFilter.Owner;
            }
            else
            {
                targetType = TargetFilter.Allies;
            }
            if (!entitiesAffected.HasFlag(targetType)) continue;
            if (!IsValidTarget(owner, proxy, entity, firstTarget)) continue;
            if (firstTarget) firstTarget = false;

            // Is it within the targeting range?
            //float distance = Vector3.Distance(entity.transform.position, owner.transform.position);
            //if (distance < targetingRange.x * owner.Stat<Stat_Effect>().aoeMultiplier) continue;
            //if (distance > targetingRange.y * owner.Stat<Stat_Effect>().aoeMultiplier) continue;

            targets.Add(entity);
        }

        Vector3 center = Vector3.zero;
        foreach (Entity entity in targets)
        {
            center += entity.transform.position;
        }
        center /= targets.Count;

        Entity centerEntity = owner.Stat<Stat_CenterEntity>().Value;
        if(centerEntity == null)
        {
            centerEntity = new GameObject().AddComponent<Entity>();
            owner.Stat<Stat_CenterEntity>().Set(centerEntity);
        }
        centerEntity.transform.position = center;
        return new List<Entity>() { centerEntity };
    }
}
