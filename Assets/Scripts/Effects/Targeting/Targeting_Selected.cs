using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(object source, Entity owner)
    {
        return owner.Stat<Stat_PlayerOwner>().player.targetType.GetTargets(source, owner);
    }

    public override List<Entity> GetTargets(object source, Trigger trigger, Entity owner)
    {
        return owner.Stat<Stat_PlayerOwner>().player.targetType.GetTargets(source, trigger, owner);
    }
}
