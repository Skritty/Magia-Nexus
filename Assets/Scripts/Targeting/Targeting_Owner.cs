using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Owner : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.GetMechanic<Mechanic_PlayerOwner>().playerEntity };
    }

    public override List<Entity> GetTargets(Effect source, Effect effect, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.GetMechanic<Mechanic_PlayerOwner>().playerEntity };
    }
}
