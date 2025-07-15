using UnityEngine;

public class Effect_ActivateTrigger : EffectTask
{
    [SerializeReference]
    public Trigger trigger;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        trigger.Invoke();
    }
}
