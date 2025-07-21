using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_TargetingMethod>().Value.GetTargets(source, owner);
    }

    public override List<Entity> GetTargets(object source, Effect effect, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_TargetingMethod>().Value.GetTargets(source, effect, owner);
    }
}
