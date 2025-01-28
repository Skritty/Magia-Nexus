using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : Effect
{
    public RuneElement element;
    public float effectMultiplierIncrease;
    public EffectTag damageTags;
    public Action opposite;

    public override void Activate()
    {
        Owner.Stat<Stat_Magic>().runes.Add(this);
        new Trigger_OnRuneUsed(this);
    }
    // Magic Effect On Hit
    public abstract void MagicEffect(DamageInstance damage);
    public abstract void MagicEffectModifier(DamageInstance damage, int currentRuneIndex);
    // Spell Shape
    public abstract void Shape(Spell spell);
    public abstract void ShapeModifier(Spell spell, int currentRuneIndex);
}

public enum RuneElement
{
    Fire, Water, Earth, Air, Order, Chaos
}