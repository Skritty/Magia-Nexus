using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredEffect : Targeting
{
    public EffectTargetingSelector selector = EffectTargetingSelector.Target;
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner, Entity proxy = null)
    {
        if (trigger.Is(out ITriggerData_Effect data))
        {
            Entity target;
            if (selector == EffectTargetingSelector.Owner)
            {
                target = data.Effect.Owner;
            }
            else
            {
                target = data.Effect.Target;
            }
            return GetTargets(source, target == null ? owner : target, proxy);
        }
        return new List<Entity>();
    }
}
