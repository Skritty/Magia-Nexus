using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_TriggeredEntity : Targeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>();
    }

    public override List<Entity> GetTargets(Effect source, Effect effect, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { effect.Owner }; // TODO rethink or remove
    }
}
