using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentEffectGate : Effect
{
    [SerializeReference, InfoBox("Ensure that this is a REFERENCE TO ANOTHER PERSISTENT EFFECT")]
    public PersistentEffect referenceEffect;
    public float requiredAmount;
    public int stacksAddedOnSuccess;
    public int stacksAddedOnFailure;

    [SerializeReference]
    public List<Effect> effectsOnSuccess = new List<Effect>();
    [SerializeReference]
    public List<Effect> effectsOnFailure = new List<Effect>();
    public override void Activate()
    {
        List<PersistentEffect> effects = Owner.Stat<Stat_PersistentEffects>().GetEffectsViaReference(referenceEffect);
        if (effects.Count > 0)
        {
            foreach(Effect effect in effectsOnSuccess)
            {
                effect.Create(Owner);
            }
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(referenceEffect, stacksAddedOnSuccess, Owner);
        }
        else
        {
            foreach (Effect effect in effectsOnFailure)
            {
                effect.Create(Owner);
            }
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(referenceEffect, stacksAddedOnFailure, Owner);
        }
    }
}
