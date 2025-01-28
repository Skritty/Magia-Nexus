using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public Entity Owner;
    // For creating the spell entity
    public CreateEntity castSpell = new CreateEntity();
    public Entity entity; // Blueprint entity to be cloned TODO: change this to be the clone as well
    public List<Rune> runes = new List<Rune>();
    public SpellShape shape;

    // What the spell entity does
    public Action spellAction;
    public DamageInstance effect = new DamageInstance();
    public int castTargets = 1;
    public int aoeTargetsModifier = 0;
    public int lifetime = 0;
    public bool destroyAfterPierces;
    public int actionsPerTurn = 1;
    public float multiplier = 0;
    public bool channeled;
    public bool castingOnChannel;
    public System.Action cleanup;

    public Spell(Entity owner, List<Rune> runes)
    {
        this.Owner = owner;
        this.runes.AddRange(runes);
    }

    public void AddGenericSpellEffect(float value, EffectModifierCalculationType calcType)
    {
        entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.Damage | EffectTag.DamageDealt, value, calcType, 1, effect);
        entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.Order | EffectTag.DamageDealt, value, calcType, 1, effect);
        entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.Knockback, value, calcType, 1, effect);
        entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.MovementSpeed, value, calcType, 1, effect);
    }

    public void AddAoESize(float value, EffectModifierCalculationType calcType)
    {
        entity.Stat<Stat_EffectModifiers>().AddModifier(EffectTag.AoE, value, calcType, 1, effect);
    }

    public void SetHitRate(int damageTicksPerTurn)
    {
        (effect as DamageInstance).ignoreFrames = GameManager.Instance.ticksPerTurn / damageTicksPerTurn;
        multiplier /= damageTicksPerTurn;
        spellAction.timing = ActionEventTiming.OnTick;
    }

    public void SetToDoT()
    {
        (effect as DamageInstance).preventTriggers = true;
        (effect as DamageInstance).skipFlatDamageReduction = true;
        multiplier /= GameManager.Instance.ticksPerTurn;
        spellAction.timing = ActionEventTiming.OnTick;
    }

    public void SetToProjectile(Targeting targeting, float baseMovementSpeed)
    {
        destroyAfterPierces = true;
        lifetime = 200;
        effect.targetSelector = targeting;
        effect.ignoreFrames = 50;
        spellAction.timing = ActionEventTiming.OnTick;
        castSpell.entityType = CreateEntity.EntityType.Projectile;
        entity.Stat<Stat_Movement>().baseMovementSpeed = baseMovementSpeed;
        entity.Stat<Stat_Projectile>().piercesRemaining = 1;
        entity.transform.localScale = Vector3.one * 0.5f;
    }

    public void GenerateSpell(Entity spellPrefab, Spell chainCast)
    {
        entity = GameObject.Instantiate(spellPrefab);
        entity.gameObject.SetActive(false);
        entity.Stat<Stat_Magic>().originSpell = this;
        spellAction = new Action();
        castSpell.projectileFanType = CreateEntity.ProjectileFanType.EvenlySpaced;

        GenerateShape();

        // Actions
        for(int i = 0; i < actionsPerTurn; i++)
        {
            entity.Stat<Stat_Actions>().AddAction(spellAction);
        }

        // Spellcast 
        spellAction.effects.Add(effect);
        castSpell.entity = entity;
        effect.effectMultiplier *= multiplier;

        // Channeling
        if (channeled)
        {
            PE_GrantChanneledAction channeledActionPE = new PE_GrantChanneledAction();
            Action channel = new Action();
            channel.effects.Add(new ChannelSpells());
            channeledActionPE.channeledAction = channel;
            channeledActionPE.Create(null, Owner, Owner);

            Trigger_OnSpellMaxStage.Subscribe(FinishChanneling);
        }

        // Spell Duration
        /*if (destroyAfterPierces)
        {
            TriggeredEffect expireTrigger = new TriggeredEffect(new Trigger_Expire(), new Expire());
            expireTrigger.triggerOrder = 99;
            expireTrigger.Create(entity);
        }*/
        if(lifetime >= 0)
        {
            PE_ExpireEntity expire = new PE_ExpireEntity();
            expire.tickDuration = lifetime + 1;
            expire.Create(entity);
        }

        // Spell Triggers
        /*if (chainCast != null)
        {
            TriggeredEffect nextSpellcastTrigger = new TriggeredEffect();
            nextSpellcastTrigger.effect = chainCast.castSpell;
            nextSpellcastTrigger.trigger = nextSpellTrigger;
            entity.Stat<Stat_PersistentEffects>().ApplyEffect(nextSpellcastTrigger);
        }*/
    }

    // TODO: Make spell contain stats about the active spell entities
    private void FinishChanneling(Trigger_Entity trigger)
    {
        spellAction.OnStart(trigger.entity);
        Owner.Stat<Stat_PersistentEffects>().RemoveEffect<PE_GrantChanneledAction>(-1);
        new Trigger_Expire(trigger.entity);
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
}