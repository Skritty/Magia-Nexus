using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : ScriptableObject
{
    public RuneElement element;
    public float magicEffectFlatDamage;
    public DamageType damageType;
    public Rune opposite;

    // Magic Effect On Hit
    public abstract void MagicEffect(DamageInstance damage, int currentRuneIndex);
    public abstract void MagicEffectModifier(DamageInstance damage, int currentRuneIndex);
    // Spell Shape
    public abstract void Shape(Spell spell, int currentRuneIndex);
    public abstract void ShapeModifier(Spell spell, int currentRuneIndex);
}

public enum RuneElement { Null, Fire, Water, Earth, Air, Chaos, Order}