using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public Entity Owner;
    // For creating the spell entity
    public CreateEntity castSpell = new CreateEntity();
    public Entity entity;
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
        this.spellAction = new Action();
        castSpell.projectileFanType = CreateEntity.ProjectileFanType.EvenlySpaced;
        CalculateDamageType();

        GenerateMagicEffect();
        GenerateShape();

        // Actions
        for(int i = 0; i < actionsPerTurn; i++)
        {
            entity.Stat<Stat_Actions>().AddAction(this.spellAction);
        }

        // Spellcast 
        spellAction.effects.Add(effect);
        castSpell.entity = entity;
        effect.effectMultiplier *= multiplier;

        // Spell Duration
        if (destroyAfterPierces)
        {
            TriggeredEffect expireTrigger = new TriggeredEffect(new Trigger_Expire(), new Expire());
            expireTrigger.triggerOrder = 99;
            expireTrigger.Create(entity);
        }
        PE_ExpireEntity expire = new PE_ExpireEntity();
        expire.tickDuration = lifetime + 1;
        expire.Create(entity);

        // Spell Triggers
        /*if (chainCast != null)
        {
            TriggeredEffect nextSpellcastTrigger = new TriggeredEffect();
            nextSpellcastTrigger.effect = chainCast.castSpell;
            nextSpellcastTrigger.trigger = nextSpellTrigger;
            entity.Stat<Stat_PersistentEffects>().ApplyEffect(nextSpellcastTrigger);
        }*/
    }

    private void GenerateMagicEffect()
    {
        for(int i = 0; i < runes.Count; i++)
        {
            if(i == Owner.Stat<Stat_Magic>().spellPhase)
                runes[i].MagicEffect(this);
            else
                runes[i].MagicEffectModifier(this);
        }
    }

    private void GenerateShape()
    {
        for (int i = 0; i < runes.Count; i++)
        {
            if(i == 0)
                runes[i].Shape(this);
            else
                runes[i].ShapeModifier(this);
        }
    }

    private void AddToDamage(EffectTag tag)
    {
        EffectTag existingTag = EffectTag.None;
        foreach (KeyValuePair<EffectTag, float> t in effect.effectTags)
        {
            existingTag = t.Key;
        }
        effect.effectTags.Remove(existingTag);
        effect.effectTags.Add(existingTag | tag, 1);
    }

    public void CalculateDamageType()
    {
        Tally<EffectTag> damageTypeTally = new Tally<EffectTag>();
        foreach (Rune effectRune in runes)
        {
            multiplier += effectRune.baseDamage;
            damageTypeTally.Add(effectRune.damageTags);
        }
        List<EffectTag> damageType = damageTypeTally.GetHighest(out _);
        if (damageType.Count >= 3)
        {
            AddToDamage(EffectTag.Magical | EffectTag.Spell);
        }
        else if (damageType.Count == 2)
        {
            EffectTag tag = damageType[0] | damageType[1];
            switch (tag)
            {
                case EffectTag.Chaos | EffectTag.Order:
                    {
                        AddToDamage(EffectTag.Damage | EffectTag.Spell);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Cold:
                    {
                        AddToDamage(EffectTag.Bludgeoning | EffectTag.Spell);
                        break;
                    }
                case EffectTag.Physical | EffectTag.Lightning:
                    {
                        AddToDamage(EffectTag.Slashing | EffectTag.Spell);
                        break;
                    }
                case EffectTag.Cold | EffectTag.Lightning:
                    {
                        AddToDamage(EffectTag.Piercing | EffectTag.Spell);
                        break;
                    }
                default:
                    {
                        //damage /= damageType.Count;
                        foreach (EffectTag t in damageType)
                        {
                            AddToDamage(t | EffectTag.Spell);
                        }
                        break;
                    }
            }
        }
        else
        {
            AddToDamage(damageType[0] | EffectTag.Spell);
        }
    }
}