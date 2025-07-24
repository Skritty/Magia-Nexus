using UnityEngine;

public class Effect_Berserk : Effect_Berserk<Effect> { }
public class Effect_Berserk<T> : EffectTask<T>
{
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (Target.Stat<Stat_Actions>().Value.Count == 0) return;
        int randomActionIndex = Random.Range(0, Target.Stat<Stat_Actions>().Value.Count);
        Action action = Target.Stat<Stat_Actions>().Value[randomActionIndex];
        Target.GetMechanic<Mechanic_Actions>().AddAction(action);
    }
}
