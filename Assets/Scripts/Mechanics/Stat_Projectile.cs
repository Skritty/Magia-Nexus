using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Projectile : Mechanic<Stat_Projectile>
{
    [FoldoutGroup("Projectile"), SerializeReference]
    public Effect effect;
    [FoldoutGroup("Projectile"), SerializeReference]
    public Targeting aoe;
    [FoldoutGroup("Projectile")]
    public int piercesRemaining;
    [FoldoutGroup("Projectile")]
    public int splitsRemaining;
    [FoldoutGroup("Projectile")]
    public Targeting_Exclude splitTargeting;

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Tick()
    {
        if (effect == null) return;
        foreach (Entity entity in aoe.GetTargets(effect, Owner))
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
        effect.Create(effect, Owner, entity);
        if (--splitsRemaining >= 0)
        {
            CreateEntity split = new CreateEntity(); // TODO: make effect have a static create
            /*Targeting_Exclude exclude = splitTargeting.Clone<Targeting_Exclude>();
            exclude.ignoredEntities.Add(entity);*/
            /*split.targetSelector = exclude;
            split.ignoreAdditionalProjectiles = true;
            split.entity = Owner;
            split.Create(Owner.Stat<Stat_PlayerOwner>().playerEntity);*/
        }
        if (--piercesRemaining < 0)
        {
            new Trigger_Expire(Owner);
        }
        else
        {
            new Trigger_ProjectilePierce(effect, effect, Owner);
        }
    }
}