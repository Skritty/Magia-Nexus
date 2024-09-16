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
        Entity target = trigger.Data<Entity>();
        return GetTargets(source, target == null ? owner : target);
    }
}
