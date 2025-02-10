using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Spell
{
    public Entity Owner; // The entity that cast the spell
    public List<Rune> runes = new List<Rune>(); // Rune formula
    public Effect effect; // The effect of the spell (generated)
    public List<Entity> origins = new List<Entity>(); // Entities that use the spell effect
    public System.Action cleanup; // Clean up any trigger subscriptions when the spell is finished

    // For creating the spell entity
    public CreateEntity castSpell = new CreateEntity();
    public Entity blueprintEntity;
    
    public SpellShape shape;

    // What the spell entity does
    public Action spellAction;
    
    public int castTargets = 1;
    public int aoeTargetsModifier = 0;
    public int lifetime = 0;
    public bool destroyAfterPierces;
    public int actionsPerTurn = 1;
    public float multiplier = 0;
    public bool channeled;
    public bool castingOnChannel;
    

    public Spell(Entity owner, List<Rune> runes)
    {
        this.Owner = owner;
        this.runes.AddRange(runes);
    }

    public void SetToProjectile(Targeting targeting, float baseMovementSpeed)
    {
        destroyAfterPierces = true;
        lifetime = 200;
        effect.targetSelector = targeting;
        effect.ignoreFrames = 50;
        spellAction.timing = ActionEventTiming.OnTick;
        castSpell.entityType = CreateEntity.EntityType.Projectile;
        blueprintEntity.Stat<Stat_Movement>().baseMovementSpeed = baseMovementSpeed;
        blueprintEntity.Stat<Stat_Projectile>().piercesRemaining = 1;
        blueprintEntity.transform.localScale = Vector3.one * 0.5f;
    }

    public void GenerateSpell(Entity spellPrefab, Spell chainCast)
    {
        blueprintEntity = GameObject.Instantiate(spellPrefab);
        blueprintEntity.gameObject.SetActive(false);
        blueprintEntity.Stat<Stat_Magic>().originSpell = this;
        spellAction = new Action();
        castSpell.projectileFanType = CreateEntity.ProjectileFanType.EvenlySpaced;

        GenerateShape();

        // Actions
        for(int i = 0; i < actionsPerTurn; i++)
        {
            blueprintEntity.Stat<Stat_Actions>().AddAction(spellAction);
        }

        // Spellcast 
        spellAction.effects.Add(effect);
        castSpell.entity = blueprintEntity;
        effect.effectMultiplier *= multiplier;

        // Channeling
        if (channeled)
        {
            PE_GrantChanneledAction channeledActionPE = new PE_GrantChanneledAction();
            Action channel = new Action();
            channel.effects.Add(new ChannelSpells());
            channeledActionPE.channeledActions = channel;
            channeledActionPE.Create(null, Owner, Owner);

            Trigger_SpellMaxStage.Subscribe(FinishChanneling, this);
        }

        // Spell Duration
        if(lifetime >= 0)
        {
            PE_ExpireEntity expire = new PE_ExpireEntity();
            expire.tickDuration = lifetime - 1;
            expire.Create(blueprintEntity);
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
    private void FinishChanneling(Trigger_SpellMaxStage trigger)
    {
        spellAction.OnStart(trigger.Owner);
        Owner.Stat<Stat_PersistentEffects>().RemoveEffect<PE_GrantChanneledAction>(-1);
        new Trigger_Expire(trigger.Owner);
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