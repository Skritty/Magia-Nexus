using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Nemesis : MultiTargeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        List<Entity> targets = new List<Entity>();
        targets.Add(Stats.GetStat<Stat_LastKilledBy>(owner).Value);

        if (targets.Count == 0)
        {
            return base.GetTargets(owner);
        }

        return targets;
    }
}
