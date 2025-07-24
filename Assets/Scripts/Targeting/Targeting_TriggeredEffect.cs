using System.Collections.Generic;

public class Targeting_TriggeredEffect : Targeting
{
    public EffectTargetingSelector selector = EffectTargetingSelector.Target;
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(object source, Effect effect, Entity owner, Entity proxy = null)
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
