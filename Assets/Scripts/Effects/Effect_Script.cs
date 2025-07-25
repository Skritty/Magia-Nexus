using UnityEngine;

public class Effect_Script<T> : EffectTask<T>
{
    public System.Action<Entity, Entity, float, bool> doEffect;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        doEffect?.Invoke(Owner, Target, multiplier, triggered);
    }
}
