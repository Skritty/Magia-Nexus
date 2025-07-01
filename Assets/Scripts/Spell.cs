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

    public void SetToProjectile()
    {
        effect.ignoreFrames = 50;
        cleanup += Trigger_ProjectileCreated.Subscribe(SetUpProjectile, effect);
    }

    private void SetUpProjectile(Trigger_ProjectileCreated trigger)
    {
        trigger.Entity.GetMechanic<Stat_Magic>().runes.AddRange(runes);
    }

    public void GenerateSpell(Effect spellcast, Spell chainCast)
    {
        this.spellcast = spellcast;
        GenerateShape();
        cleanup += Trigger_Hit.Subscribe((x) => new Trigger_SpellEffectApplied(x.Effect, Owner, this, effect, Owner, this), effect);
        Owner.GetMechanic<Stat_Magic>().ownedSpells.Add(this);
        if (!channeled)
        {
            CastFromProxies();
        }
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
        Owner.GetMechanic<Stat_Actions>().channelInstead = true;
        cleanup += Trigger_Channel.Subscribe(ChannelSpell, Owner);
        cleanup += Trigger_SpellMaxStage.Subscribe(FinishChanneling, this);
    }

    private void ChannelSpell(Trigger_Channel trigger)
    {
        Stage++;
    }

    private void FinishChanneling(Trigger_SpellMaxStage trigger)
    {
        trigger.Spell.Owner.GetMechanic<Stat_Actions>().channelInstead = false;
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
        new Trigger_SpellCast(Owner, this, Owner, this);
    }

    public void CastSpell(Entity proxy)
    {
        int targets = additionalCastTargets + (int)Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.CastTargets);
        targets = Mathf.Clamp(targets, 1, int.MaxValue);
        for (int t = 0; t < targets; t++)
        {
            effect.Create(Owner, proxy);
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
        new Trigger_SpellFinished(Owner, this, Owner, this);
        cleanup?.Invoke();
        Owner.GetMechanic<Stat_Magic>().ownedSpells.Remove(this);
    }
}