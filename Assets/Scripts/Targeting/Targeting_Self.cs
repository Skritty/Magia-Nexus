using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Self : Targeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner };
    }
}
