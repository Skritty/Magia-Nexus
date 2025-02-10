using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Exclude : Targeting
{
    [SerializeReference]
    public Targeting targeting;
    public List<Entity> ignoredEntities;
    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        List<Entity> targets = targeting.GetTargets(source, owner);
        targets.RemoveAll(x => ignoredEntities.Contains(x));
        return targets;
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        List<Entity> targets = targeting.GetTargets(source, owner);
        targets.RemoveAll(x => ignoredEntities.Contains(x));
        return targets;
    }
}
