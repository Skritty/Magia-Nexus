using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredEntity : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        return new List<Entity>();
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        Trigger_Entity entityTrigger = trigger as Trigger_Entity;
        if (entityTrigger == null) return new List<Entity>();
        return new List<Entity>() { entityTrigger.entity };
    }
}
