using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public Entity owner;
    // For creating the spell entity
    public CreateEntity castSpell = new CreateEntity();
    public Entity entity;
    public Rune effectRune;
    public List<Rune> effectModifiers = new List<Rune>();
    public List<Rune> spellModifiers = new List<Rune>();
    public List<Rune> shapeModifiers = new List<Rune>();
    public Trigger nextSpellTrigger;

    // What the spell entity does
    public Action spellAction;
    public Effect effect;
    public int targets = 1;
    public int lifetime = 125;

    public Spell(Rune effect, List<Rune> effectModifiers)
    {
        this.effectRune = effect;
        this.effectModifiers.AddRange(effectModifiers);
        this.effectModifiers.RemoveAt(0);
    }

    public void GenerateSpell(Entity spellPrefab, Action spellAction, Spell chainCast)
    {
        entity = GameObject.Instantiate(spellPrefab);
        entity.gameObject.SetActive(false);
        this.spellAction = GameObject.Instantiate(spellAction);
        entity.Stat<Stat_Actions>().AddRepeatAction(this.spellAction);
        PE_ExpireEntity expire = new PE_ExpireEntity();
        expire.tickDuration = lifetime;
        expire.Create(this, entity);


        GenerateSpellEffect();
        GenerateTargeting();
        castSpell.entity = entity;

        if (chainCast != null)
        {
            TriggeredEffect nextSpellcastTrigger = new TriggeredEffect();
            nextSpellcastTrigger.effect = chainCast.castSpell;
            nextSpellcastTrigger.trigger = nextSpellTrigger;
            entity.Stat<Stat_PersistentEffects>().ApplyEffect(nextSpellcastTrigger);
        }
    }

    private void GenerateSpellEffect()
    {
        effectRune.EffectFormula(this, null);
        EffectFormula(effectRune, 0);
        effectRune.FinalizeEffectFormula(this);
        spellAction.effects.Add(effect);
    }

    private void EffectFormula(Rune previousRune, int index)
    {
        if (effectModifiers.Count == index) return;
        EffectFormula(previousRune.EffectFormula(this, effectModifiers[index]), ++index);
    }

    private void GenerateTargeting()
    {
        shapeModifiers[0].TargetingFormula(this, null);
        TargetingFormula(shapeModifiers[0], 1);
    }

    private void TargetingFormula(Rune previousRune, int index)
    {
        // (Water + Earth = Earth) + Fire = Fire -> projectiles = 5, each spell triggers AoE circle on hit (0 damage hit)
        // Water.GenerateAoE(Earth) => Earth, 5 spells in list | Earth.GenerateAoE(Fire) => AoE, Applies Trigger to all spells
        if (shapeModifiers.Count == index) return;
        TargetingFormula(previousRune.TargetingFormula(this, shapeModifiers[index]), ++index);
    }
}