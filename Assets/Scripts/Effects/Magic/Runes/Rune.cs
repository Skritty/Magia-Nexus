using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : Effect
{
    public RuneElement element;
    public float magicEffectFlatDamage;
    public DamageType damageType;
    public Action opposite;

    public override void Activate()
    {
        Owner.Stat<Stat_Magic>().AddRune(this);
        new Trigger_RuneUsed(this, this, Target);
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
    Fire, Water, Earth, Air, Chaos, Order
}