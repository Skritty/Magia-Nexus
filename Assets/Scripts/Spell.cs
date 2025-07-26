using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : Effect
{
    public List<Rune> runes = new List<Rune>(); // Rune formula
    public EffectTask<Effect> effect; // The effect of the spell (generated)
    public List<Entity> proxies = new List<Entity>(); // Entities that use the spell effect
    public System.Action cleanup; // Clean up any trigger subscriptions when the spell is finished
    public int maxStages;
    private int _stage;
    public int Stage
    {
        get => _stage;
        set
        {
            _stage = value;
            Trigger_SpellStageIncrement.Invoke(this, Owner, this);
            if (_stage == maxStages)
            {
                Trigger_SpellMaxStage.Invoke(this, Owner, this);
            }
        }
    }

    // Spell Generating Info
    public SpellShape shape;
    public bool channeled;
    public bool addRunesToHit = true;
    public bool ignoreMaxStageCast;
    public int chainsRemaining;
    public int additionalCastTargets;
    public int proxyLifetime;

    public Spell(Entity owner, Rune[] runes)
    {
        this.Owner = owner;
        this.runes.AddRange(runes);
    }

    public void SetToProjectile()
    {
        effect.ignoreFrames = 50;
        cleanup += Trigger_ProjectileCreated.Subscribe(SetUpProjectile, effect);
    }

    private void SetUpProjectile(Entity entity)
    {
        entity.Stat<Stat_Runes>().AddModifiers(runes);
    }

    public void GenerateSpell(Spell chainCast)
    {
        GenerateShape();
        SubscribeOnHit(x => x.EffectMultiplier = EffectMultiplier);
        if (addRunesToHit) SubscribeOnHit(x => x.runes.AddRange(runes));
        SubscribeOnHit(x => Trigger_SpellEffectApplied.Invoke(x, Owner, this, effect, Owner, this));
        Owner.GetMechanic<Mechanic_Magic>().ownedSpells.Add(this);
        if (!channeled) CastFromProxies();
    }

    public void SubscribeOnHit(Action<Hit> action)
    {
        cleanup += Trigger_PreHit.Subscribe(action, effect);
    }

    public void AddChaining(int additionalBranches)
    {
        if(chainsRemaining == 0)
        {
            cleanup += Trigger_PostHit.Subscribe(ChainCast);
        }
        chainsRemaining += additionalBranches;
    }

    private void ChainCast(Hit hit)
    {
        if (--chainsRemaining >= 0)
        {
            CastSpell(hit.Target);
        }
    }

    public void SetCastOnStageGained()
    {
        ignoreMaxStageCast = true;
        cleanup += Trigger_SpellStageIncrement.Subscribe(x => CastFromProxies(), this, 5);
    }

    public void SetChannelSpell(int maxStages)
    {
        this.maxStages = maxStages;
        channeled = true;
        System.Action removeChanneling = Owner.Stat<Stat_Channeling>().AddModifier(true);
        cleanup += Trigger_Channel.Subscribe(ChannelSpell, Owner);
        cleanup += Trigger_SpellMaxStage.Subscribe(_ => FinishChanneling(removeChanneling), this);
    }

    private void ChannelSpell(Entity entity)
    {
        Stage++;
    }

    private void FinishChanneling(System.Action removeChanneling)
    {
        removeChanneling.Invoke();
        if (!ignoreMaxStageCast)
        {
            CastFromProxies();
        }
        StopSpell();
    }

    public void CastFromProxies()
    {
        foreach (Entity proxy in proxies)
        {
            CastSpell(proxy);
        }
        Trigger_SpellCast.Invoke(this, Owner, this);
    }

    public void CastSpell(Entity proxy)
    {
        int targets = additionalCastTargets + (int)Owner.Stat<Stat_CastTargets>().Value;
        targets = Mathf.Clamp(targets, 1, int.MaxValue);
        for (int t = 0; t < targets; t++)
        {
            effect.DoTask(null, Owner, proxy);
        }
    }

    private void GenerateShape()
    {
        for (int i = 0; i < runes.Count; i++)
        {
            if(i == 0)
                runes[i].Shape(this, i);
            else
                runes[i].ShapeModifier(this, i);
        }
    }

    public void StopSpell()
    {
        Trigger_SpellFinished.Invoke(this, Owner, this);
        cleanup?.Invoke();
        Owner.GetMechanic<Mechanic_Magic>().ownedSpells.Remove(this);
    }
}