using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredEntity : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>();
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner, Entity proxy = null)
    {
        if(trigger.Is(out IDataContainer_OwnerEntity data))
            return new List<Entity>() { data.Entity };
        return new List<Entity>();
    }
}
