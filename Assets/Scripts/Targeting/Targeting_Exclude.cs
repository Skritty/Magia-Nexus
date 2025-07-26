using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Exclude : Targeting
{
    [SerializeReference]
    public Targeting targeting;
    public List<Entity> ignoredEntities;
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        List<Entity> targets = targeting.GetTargets(source, owner);
        targets.RemoveAll(x => ignoredEntities.Contains(x));
        return targets;
    }
}
