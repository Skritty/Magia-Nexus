using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_Projectiles : NumericalSolver, IStat<float> { }
public class Stat_PiercesRemaining : NumericalSolver, IStat<float> { }
public class Stat_SplitsRemaining : NumericalSolver, IStat<float> { }
public class Stat_AdditionalSplits : NumericalSolver, IStat<float> { }
//public class Stat_ProjectileTargetingAoE : PrioritySolver<Targeting>, IStatTag { }
public class Mechanic_Projectile : Mechanic
{
    [FoldoutGroup("Projectile")]
    public Entity prefab;
    [FoldoutGroup("Projectile"), SerializeReference]
    public Targeting aoe;
    [FoldoutGroup("Projectile")]
    public Effect_Projectile splitProjectile;
    [FoldoutGroup("Projectile"), SerializeReference]
    public List<EffectTask> tasks = new();

    public override void Tick()
    {
        if (tasks.Count == 0) return;
        foreach (Entity target in aoe.FindTargets(Owner))
        {
            OnHit(target);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnHit(Entity target)
    {
        foreach (EffectTask task in tasks)
        {
            if (!task.DoTask(Owner, target, null, false)) break;
        }

        if (splitProjectile != null && Stats.GetStat<Stat_SplitsRemaining>(Owner).Value > 0)
        {
            Stats.GetStat<Stat_SplitsRemaining>(Owner).Add(-1);

            Effect_Projectile split = (Effect_Projectile)splitProjectile.Clone();
            
            split.targetSelector.targetingConditions.Add(new TargetingCondition_Exclude(target));
            split.numberOfProjectiles += (int)Stats.GetStat<Stat_AdditionalSplits>(Stats.GetStat<Stat_PlayerCharacter>(Owner).Value).Value;
            split.DoTask(Owner);

            Trigger_Expire.Invoke(Owner, Owner);
            return;
        }

        if (Stats.GetStat<Stat_PiercesRemaining>(Owner).Value > 0)
        {
            Stats.GetStat<Stat_PiercesRemaining>(Owner).Add(-1);
            Trigger_ProjectilePierce.Invoke(Owner, Owner);
            return;
        }

        Trigger_Expire.Invoke(Owner, Owner);
    }
}