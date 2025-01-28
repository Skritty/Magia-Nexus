using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredEffect : Targeting
{
    public EffectTargetingSelector selector = EffectTargetingSelector.Target;
    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        Trigger_Effect damageTrigger = trigger as Trigger_Effect;
        if(damageTrigger == null) return new List<Entity>();
        Entity target;
        if (selector == EffectTargetingSelector.Owner)
        {
            target = damageTrigger.effect.Owner;
        }
        else
        {
            target = damageTrigger.effect.Target;
        }
        
        return GetTargets(source, target == null ? owner : target);
    }
}
