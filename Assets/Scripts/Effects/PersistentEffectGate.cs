using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentEffectGate : FlipFlopPersistentEffect
{
    public float requiredAmount;
    public int stacksAddedOnSuccess;
    public int stacksAddedOnFailure;

    [SerializeReference]
    public List<Effect> effectsOnSuccess = new List<Effect>();
    [SerializeReference]
    public List<Effect> effectsOnFailure = new List<Effect>();
    public override void Activate()
    {
        List<PersistentEffect> effects = Owner.Stat<Stat_PersistentEffects>().GetEffectsViaReference(persistentEffect);
        if (effects.Count > 0)
        {
            foreach(Effect effect in effectsOnSuccess)
            {
                effect.Create(Owner);
            }
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(persistentEffect, stacksAddedOnSuccess, Owner);
        }
        else
        {
            foreach (Effect effect in effectsOnFailure)
            {
                effect.Create(Owner);
            }
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(persistentEffect, stacksAddedOnFailure, Owner);
        }
    }
}
