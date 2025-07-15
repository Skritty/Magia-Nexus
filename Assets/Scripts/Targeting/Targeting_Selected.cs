using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_SelectedTargeting>().Value.GetTargets(source, owner);
    }

    public override List<Entity> GetTargets(Effect source, Effect effect, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_SelectedTargeting>().Value.GetTargets(source, effect, owner);
    }
}
