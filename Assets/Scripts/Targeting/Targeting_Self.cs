using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Self : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner)
    {
        return new List<Entity>() { owner };
    }

    public override List<Entity> GetTargets(Effect source, Trigger trigger, Entity owner)
    {
        return new List<Entity>() { owner };
    }
}
