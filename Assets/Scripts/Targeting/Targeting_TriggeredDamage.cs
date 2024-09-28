using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTargetingSelector { Owner, Target }
public class Targeting_TriggeredDamage : Targeting
{
    public EffectTargetingSelector selector = EffectTargetingSelector.Target;
    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        Entity target;
        if (selector == EffectTargetingSelector.Owner)
        {
            target = trigger.Data<DamageInstance>().Owner;
        }
        else
        {
            target = trigger.Data<DamageInstance>().Target;
        }
         
        return GetTargets(source, target == null ? owner : target);
    }
}
