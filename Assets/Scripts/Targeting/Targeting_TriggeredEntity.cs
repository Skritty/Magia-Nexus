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
        if(trigger.Is(out ITriggerData_OwnerEntity data))
            return new List<Entity>() { data.Owner };
        return new List<Entity>();
    }
}
