using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class CleanupHelper : ListStat<Action> { }
public class SetTargets : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Targeting targeting;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        foreach(Action action in $"SetTargets{owner}{target}".GetStat<CleanupHelper>())
        {
            action?.Invoke();
        }
        $"SetTargets{owner}{target}".GetStat<CleanupHelper>().Clear();
        byte priority = 0;
        foreach(Entity t in targeting.FindTargets(target))
        {
            Action cleanup = target.GetStat<Stat_Targets>().Add(t, priority);
            Trigger_Die.Subscribe(x =>
            {
                if (x.Target == t) cleanup?.Invoke();
            }, t);
            $"SetTargets{owner}{target}".GetStat<CleanupHelper>().Add(cleanup);
        }
    }
}