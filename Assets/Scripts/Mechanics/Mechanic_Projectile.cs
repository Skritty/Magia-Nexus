using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_Projectiles : NumericalSolver, IStatTag<float> { }
public class Stat_PiercesRemaining : NumericalSolver, IStatTag<float> { }
public class Stat_SplitsRemaining : NumericalSolver, IStatTag<float> { }
public class Stat_AdditionalSplits : NumericalSolver, IStatTag<float> { }
//public class Stat_ProjectileTargetingAoE : PrioritySolver<Targeting>, IStatTag { }
public class Mechanic_Projectile : Mechanic<Mechanic_Projectile>
{
    [FoldoutGroup("Projectile")]
    public Entity prefab;
    [FoldoutGroup("Projectile"), SerializeReference]
    public Targeting aoe;
    [FoldoutGroup("Projectile")]
    public Effect_CreateEntity splitProjectile;
    [FoldoutGroup("Projectile"), SerializeReference]
    public List<EffectTask> tasks = new();

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Tick()
    {
        if (tasks.Count == 0) return;
        foreach (Entity target in aoe.GetTargets(this, Owner))
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
            if (!task.DoTask(null, target)) break;
        }

        if (splitProjectile != null && Owner.Stat<Stat_SplitsRemaining>().Value > 0)
        {
            Owner.Stat<Stat_SplitsRemaining>().AddModifier(-1);

            Effect_CreateEntity split = (Effect_CreateEntity)splitProjectile.Clone();
            (split.targetSelector as Targeting_Exclude)?.ignoredEntities.Add(target);
            split.numberOfProjectiles += (int)Owner.Stat<Stat_PlayerCharacter>().Value.Stat<Stat_AdditionalSplits>().Value;
            split.DoTask(null, Owner);

            Trigger_Expire.Invoke(Owner, Owner);
            return;
        }

        if (Owner.Stat<Stat_PiercesRemaining>().Value > 0)
        {
            Owner.Stat<Stat_PiercesRemaining>().AddModifier(-1);
            Trigger_ProjectilePierce.Invoke(Owner, Owner);
            return;
        }

        Trigger_Expire.Invoke(Owner, Owner);
    }
}