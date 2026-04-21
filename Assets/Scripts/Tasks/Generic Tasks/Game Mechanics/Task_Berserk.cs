using UnityEngine;

public class Task_Berserk : EffectTask
{
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (Stats.GetStat<Stat_Actions>(owner).Value.Count == 0) return;
        int randomActionIndex = Random.Range(0, Stats.GetStat<Stat_Actions>(owner).Value.Count);
        Action action = Stats.GetStat<Stat_Actions>(owner).Value[randomActionIndex];
        Stats.GetStat<Mechanic_Actions>(owner).AddAction(action);
    }
}
