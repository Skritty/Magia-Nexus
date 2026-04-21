using System.Collections.Generic;
public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        return Stats.GetStat<Stat_TargetingMethod>(owner).Value.GetTargets(owner, proxy);
    }

    public override List<Entity> GetTargets<T>(T triggerData, Entity owner, Entity proxy = null)
    {
        return Stats.GetStat<Stat_TargetingMethod>(owner).Value.GetTargets(triggerData, owner, proxy);
    }
}
