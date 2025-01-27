using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class DamageInstance : Effect
{
    protected override bool UsedInCalculations => true;

    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new List<Rune>(); 
    [FoldoutGroup("@GetType()")]
    public int ignoreFrames;
    [FoldoutGroup("@GetType()")]
    public bool skipFlatDamageReduction;
    [FoldoutGroup("@GetType()")]
    public bool preventTriggers;
    
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> ownerEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> targetEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()")]
    public List<Effect> onHitEffects = new();
    [SerializeReference, FoldoutGroup("@GetType()"), InfoBox("These are in effect until the end of damage calculation")]
    public List<TriggeredEffect> temporaryTriggeredEffects = new();

    public override void Activate()
    {
        if (Target.Stat<Stat_Life>().maxLife <= 0) return;
        foreach (TriggeredEffect effect in temporaryTriggeredEffects)
        {
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, effect.stacks, Owner);
        }
        if (!preventTriggers)
        {
            foreach (Effect effect in targetEffects)
            {
                effect.Create(Source, Owner, Target);
            }
            foreach (Effect effect in ownerEffects)
            {
                effect.Create(Source, Owner, Owner);
            }
            foreach (Effect effect in onHitEffects)
            {
                effect.Create(this);
            }
            foreach (PE_RuneCrystal crystal in Target.Stat<Stat_PersistentEffects>().GetEffects<PE_RuneCrystal>())
            {
                runes.Add(crystal.rune);
                Target.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(crystal, -1);
            }
            GenerateMagicEffect(runes);
            Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_OnHit>(this, this));
            Target.Trigger<Trigger_WhenHit>(this, this);
        }

        Target.Stat<Stat_Life>().TakeDamage(this);

        if (ignoreFrames > 0)
            new PE_IgnoreEntity(ignoreFrames).Create(this);

        foreach (TriggeredEffect effect in temporaryTriggeredEffects)
        {
            Owner.Stat<Stat_PersistentEffects>().AddOrRemoveSimilarEffect(effect, -effect.stacks, Owner);
        }
    }

    public void GenerateMagicEffect(List<Rune> runes)
    {
        for (int i = 0; i < runes.Count; i++)
        {
            if (i == Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.SpellPhase) % runes.Count)
                runes[i].MagicEffect(this);
            else
                runes[i].MagicEffectModifier(this, i);// TODO pass in rune list here
        }
    }

    private void AddToDamage(EffectTag tag)
    {
        effectTags.TryAdd(tag, 0);
        foreach (KeyValuePair<EffectTag, float> t in effectTags)
        {
            effectTags[t.Key] = 1f / effectTags.Count;
        }
    }

    public void CalculateDamageType()
    {
        float increase = 1;
        Tally<EffectTag> damageTypeTally = new Tally<EffectTag>();
        foreach (Rune effectRune in runes)
        {
            increase += effectRune.effectMultiplierIncrease;
            damageTypeTally.Add(effectRune.damageTags);
        }
        effectMultiplier *= increase;
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

    public new DamageInstance Clone()
    {
        DamageInstance clone = (DamageInstance)base.Clone();
        clone.ownerEffects = ownerEffects;
        clone.targetEffects = targetEffects;
        clone.temporaryTriggeredEffects = temporaryTriggeredEffects;
        return clone;
    }
}