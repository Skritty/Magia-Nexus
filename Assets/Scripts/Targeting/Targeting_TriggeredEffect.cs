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

    public override List<Entity> GetTargets(Effect source, Effect effect, Entity owner, Entity proxy = null)
    {
        Entity target;
        if (selector == EffectTargetingSelector.Owner)
        {
            target = effect.Owner;
        }
        else
        {
            target = effect.Target;
        }
        return GetTargets(source, target == null ? owner : target, proxy);
    }
}
