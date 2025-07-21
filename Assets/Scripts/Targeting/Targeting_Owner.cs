using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Owner : Targeting
{
    public override List<Entity> GetTargets(object source, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.Stat<Stat_PlayerCharacter>().Value };
    }

    public override List<Entity> GetTargets(object source, Effect effect, Entity owner, Entity proxy = null)
    {
        return new List<Entity>() { owner.Stat<Stat_PlayerCharacter>().Value };
    }
}
