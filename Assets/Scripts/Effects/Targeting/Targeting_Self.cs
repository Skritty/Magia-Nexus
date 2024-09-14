using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Self : Targeting
{
    public override List<Entity> GetTargets(object source, Entity owner)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(object source, Trigger trigger, Entity owner)
    {
        return GetTargets(source, owner);
    }
}
