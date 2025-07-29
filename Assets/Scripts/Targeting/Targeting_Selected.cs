using System.Collections.Generic;
using Unity.VisualScripting;
public class Stat_LockedTargets : Stat<List<Entity>>, IStatTag<List<Entity>> { }
public class Targeting_Selected : Targeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        if (owner.GetMechanic<Mechanic_PlayerOwner>().player.lockTargeting)
        {
            List<Entity> lockedTargets = owner.Stat<Stat_LockedTargets>().Value;
            if(lockedTargets == null)
            {
                lockedTargets = new List<Entity>();
                owner.Stat<Stat_LockedTargets>().AddModifier(lockedTargets);
            }
            lockedTargets.RemoveAll(x => x == null || x.gameObject.activeSelf == false);
            if(lockedTargets.Count == 0)
            {
                lockedTargets.AddRange(owner.Stat<Stat_TargetingMethod>().Value.GetTargets(owner));
            }
            return lockedTargets;
        }
        
        return owner.Stat<Stat_TargetingMethod>().Value.GetTargets(owner, proxy);
    }

    public override List<Entity> GetTargets<T>(T triggerData, Entity owner, Entity proxy = null)
    {
        if (owner.GetMechanic<Mechanic_PlayerOwner>().player.lockTargeting)
        {
            List<Entity> lockedTargets = owner.Stat<Stat_LockedTargets>().Value;
            if (lockedTargets == null)
            {
                lockedTargets = new List<Entity>();
                owner.Stat<Stat_LockedTargets>().AddModifier(lockedTargets);
            }
            lockedTargets.RemoveAll(x => x == null || x.gameObject.activeSelf == false);
            if (lockedTargets.Count == 0)
            {
                lockedTargets.AddRange(owner.Stat<Stat_TargetingMethod>().Value.GetTargets(owner));
            }
            return lockedTargets;
        }

        return owner.Stat<Stat_TargetingMethod>().Value.GetTargets(triggerData, owner, proxy);
    }
}
