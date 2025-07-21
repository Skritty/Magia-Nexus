using UnityEngine;

public class Effect_DoHit : EffectTask
{
    public Hit hit;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Hit clone = hit.Clone();
        clone.EffectMultiplier = multiplier;
        clone.Source = this;
        clone.Owner = Owner;
        clone.Target = Target;

        if (!triggered)
        {
            clone.PreHitTriggers(); 
            clone.PostHitTriggers();
        }
    }
}
