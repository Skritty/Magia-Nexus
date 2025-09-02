using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Nemesis : MultiTargeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        List<Entity> targets = new List<Entity>();
        targets.Add(owner.Stat<Stat_LastKilledBy>().Value);

        if (targets.Count == 0)
        {
            return base.GetTargets(owner);
        }

        return targets;
    }
}
