using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_Targeting>().targetingType.GetTargets(source, owner);
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner, Entity proxy = null)
    {
        return owner.Stat<Stat_Targeting>().targetingType.GetTargets(source, trigger, owner);
    }
}
