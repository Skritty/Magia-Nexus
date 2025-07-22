using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_PiercesRemaining : NumericalSolver, IStatTag { }
public class Stat_SplitsRemaining : NumericalSolver, IStatTag { }
//public class Stat_ProjectileTargetingAoE : PrioritySolver<Targeting>, IStatTag { }
public class Mechanic_Projectile : Mechanic<Mechanic_Projectile>
{
    [FoldoutGroup("Projectile"), SerializeReference]
    public List<EffectTask> tasks = new();
    [FoldoutGroup("Projectile"), SerializeReference]
    public Targeting aoe;
    [FoldoutGroup("Projectile")]
    public Targeting_Exclude splitTargeting;

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Tick()
    {
        if (tasks.Count == 0) return;
        foreach (Entity entity in aoe.GetTargets(this, Owner))
        {
            OnHit(entity);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnHit(Entity entity)
    {
        foreach (EffectTask task in tasks)
        {
            if (!task.DoTask(null, entity)) break;
        }

        Owner.Stat<Stat_SplitsRemaining>().AddModifier(-1);
        if (Owner.Stat<Stat_SplitsRemaining>().Value >= 0)
        {
            Effect_CreateEntity split = new Effect_CreateEntity(); // TODO: make effect have a static create
            /*Targeting_Exclude exclude = splitTargeting.Clone<Targeting_Exclude>();
            exclude.ignoredEntities.Add(entity);*/
            /*split.targetSelector = exclude;
            split.ignoreAdditionalProjectiles = true;
            split.entity = Owner;
            split.Create(Owner.Stat<Stat_PlayerOwner>().playerEntity);*/
        }

        Owner.Stat<Stat_PiercesRemaining>().AddModifier(-1);
        if (Owner.Stat<Stat_PiercesRemaining>().Value < 0)
        {
            Trigger_Expire.Invoke(Owner, Owner);
        }
        else
        {
            Trigger_ProjectilePierce.Invoke(Owner, Owner);
        }
    }
}