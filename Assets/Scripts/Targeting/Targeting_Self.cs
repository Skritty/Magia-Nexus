using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Self : Targeting
{
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(object source, Effect effect, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }
}
