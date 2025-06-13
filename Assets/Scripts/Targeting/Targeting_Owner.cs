using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Owner : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.Stat<Stat_PlayerOwner>().playerEntity };
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.Stat<Stat_PlayerOwner>().playerEntity };
    }
}
