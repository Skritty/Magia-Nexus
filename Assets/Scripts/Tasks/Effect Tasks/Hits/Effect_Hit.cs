using Sirenix.OdinInspector;
using UnityEngine;
public class Effect_Hit : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Hit hit;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        Hit clone = hit.Clone();
        clone.EffectMultiplier *= multiplier;
        clone.Owner = owner;
        clone.Target = target;

        if (!triggered)
        {
            clone.PreHitTriggers(); 
            clone.PostHitTriggers();
        }
    }
}
