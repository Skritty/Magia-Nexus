using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class FlipFlopPersistentEffect : Effect
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public PersistentEffect persistentEffect;
    public override void Activate()
    {
        List<PersistentEffect> effects = Owner.GetMechanic<Stat_PersistentEffects>().GetEffectsViaReference(persistentEffect);
        if (effects.Count > 0)
        {
            Owner.GetMechanic<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(persistentEffect, -1, Owner);
        }
        else
        {
            Owner.GetMechanic<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(persistentEffect, 1, Owner);
        }
    }
}
