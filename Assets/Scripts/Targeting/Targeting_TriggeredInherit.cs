using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredInherit : Targeting
{
    public EffectTargetingSelector selector = EffectTargetingSelector.Target;
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets<T>(object source, T triggeredData, Entity owner, Entity proxy = null)
    {
        if (triggeredData is Entity)
        {
            return new List<Entity>() { triggeredData as Entity };
        }
        else if(triggeredData is Effect)
        {
            Entity target;
            if (selector == EffectTargetingSelector.Owner)
            {
                target = (triggeredData as Effect).Owner;
            }
            else
            {
                target = (triggeredData as Effect).Target;
            }
            return GetTargets(source, target == null ? owner : target, proxy);
        }
        else 
        {
            return GetTargets(source, owner, proxy);
        }
    }
}
