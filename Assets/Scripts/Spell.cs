using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public Entity Owner; // The entity that cast the spell
    public List<Rune> runes = new List<Rune>(); // Rune formula
    public Effect effect; // The effect of the spell (generated)
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
            new Trigger_SpellStageIncrement(Owner, this, Owner, this);
            if (_stage == maxStages)
            {
                new Trigger_SpellMaxStage(Owner, this, Owner, this);
            }
        }
    }

    // Spell Generating Info
    public Effect spellcast;
    public SpellShape shape;
    public bool channeled;
    public bool ignoreMaxStageCast;
    public int chainsRemaining;
    public int additionalCastTargets;
    public int proxyLifetime;

    public Spell(Entity owner, List<Rune> runes)
    {
        this.Owner = owner;
        this.runes.AddRange(runes);
    }

    public void SetToProjectile(CreateEntity createProxies)
    {
        effect.ignoreFrames = 50;
        cleanup += Trigger_ProjectileCreated.Subscribe(SetUpProjectile, createProxies);
    }

    private void SetUpProjectile(Trigger_ProjectileCreated trigger)
    {
        trigger.Entity.Stat<Stat_Magic>().runes.AddRange(runes);
        trigger.Entity.Stat<Stat_Magic>().useRunesToEnchantAttacks = true;
        trigger.Entity.Stat<Stat_Magic>().consumeRunesOnEnchant = false;
    }

    public void GenerateSpell(Effect spellcast, Spell chainCast)
    {
        this.spellcast = spellcast;
        GenerateShape();
        cleanup += Trigger_Hit.Subscribe((x) => new Trigger_SpellEffectApplied(x.Effect, Owner, this, effect, Owner, this), effect);
        (effect.targetSelector as MultiTargeting).numberOfTargets += Mathf.Clamp(additionalCastTargets, 1, int.MaxValue);
        if (!channeled) CastFromProxies();
    }

    public void AddChaining(int additionalBranches)
    {
        if(chainsRemaining == 0)
        {
            cleanup += Trigger_Hit.Subscribe(ChainCast);
        }
        chainsRemaining += additionalBranches;
    }

    private void ChainCast(Trigger_Hit trigger)
    {
        if (--chainsRemaining >= 0)
        {
            CastSpell(trigger.Effect.Target);
        }
    }

    public void AddRunesToDamageInstance(DamageInstance damage)
    {
        damage.runes.AddRange(runes);
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
        Owner.Stat<Stat_Actions>().channelInstead = true;
        cleanup += Trigger_Channel.Subscribe(ChannelSpells, Owner);
        cleanup += Trigger_SpellMaxStage.Subscribe(FinishChanneling, this);
    }

    private void ChannelSpells(Trigger_Channel trigger)
    {
        foreach (Spell spell in trigger.Entity.Stat<Stat_Magic>().ownedSpells)
        {
            if(!spell.channeled)
            spell.Stage++;
        }
    }

    private void FinishChanneling(Trigger_SpellMaxStage trigger)
    {
        trigger.Spell.Owner.Stat<Stat_Actions>().channelInstead = false;
        if (ignoreMaxStageCast) return;
        CastFromProxies();
    }

    public void CastFromProxies()
    {
        foreach (Entity proxy in proxies)
        {
            CastSpell(proxy);
        }
    }

    public void CastSpell(Entity proxy)
    {
        for (int targets = 0; targets < additionalCastTargets + Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.CastTargets); targets++)
        {
            effect.Create(Owner, proxy);
        }
    }

    private void GenerateShape()
    {
        for (int i = 0; i < runes.Count; i++)
        {
            if(i == 0)
                runes[i].Shape(this);
            else
                runes[i].ShapeModifier(this, i);
        }
    }

    public void StopSpell()
    {
        cleanup?.Invoke();
    }
}