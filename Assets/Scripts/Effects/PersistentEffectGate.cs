using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentEffectGate : Effect
{
    [SerializeReference, InfoBox("Ensure that this is a REFERENCE TO ANOTHER PERSISTENT EFFECT")]
    public PersistentEffect referenceEffect;
    public float requiredAmount;
    public float addedOnSuccess;
    public float addedOnFailure;

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
            for(int i = 0; i < Mathf.Abs(addedOnSuccess); i++)
            {
                if(addedOnSuccess > 0)
                {
                    referenceEffect.Create(Owner);
                }
                else
                {
                    Owner.Stat<Stat_PersistentEffects>().RemoveSimilarEffect(referenceEffect, Owner);
                }
            }
        }
        else
        {
            foreach (Effect effect in effectsOnFailure)
            {
                effect.Create(Owner);
            }
            for (int i = 0; i < Mathf.Abs(addedOnFailure); i++)
            {
                if (addedOnFailure > 0)
                {
                    referenceEffect.Create(Owner);
                }
                else
                {
                    Owner.Stat<Stat_PersistentEffects>().RemoveSimilarEffect(referenceEffect, Owner);
                }
            }
        }
    }
}
