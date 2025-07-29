using UnityEngine;

public class Task_Berserk : EffectTask
{
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (owner.Stat<Stat_Actions>().Value.Count == 0) return;
        int randomActionIndex = Random.Range(0, owner.Stat<Stat_Actions>().Value.Count);
        Action action = owner.Stat<Stat_Actions>().Value[randomActionIndex];
        owner.GetMechanic<Mechanic_Actions>().AddAction(action);
    }
}
