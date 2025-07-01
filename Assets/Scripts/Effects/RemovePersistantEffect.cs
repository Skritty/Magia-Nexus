using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovePersistantEffect : Effect
{
    public int stacksRemoved = 1;
    public PersistentEffect.Alignment alignmentRemoved;
    public override void Activate()
    {
        Target.GetMechanic<Stat_PersistentEffects>().RemoveRandomEffect(alignmentRemoved, stacksRemoved);
    }
}
