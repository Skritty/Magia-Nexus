using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_DoHit : Effect_DoHit<Effect> { }
public class Effect_DoHit<T> : EffectTask<T>
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Hit hit;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Hit clone = hit.Clone();
        clone.EffectMultiplier = multiplier;
        clone.Owner = Owner;
        clone.Target = Target;

        if (!triggered)
        {
            clone.PreHitTriggers(); 
            clone.PostHitTriggers();
        }
    }
}
