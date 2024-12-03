using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : Effect
{
    public float baseDamage;
    public EffectTag damageTags;
    public Action opposite;

    public override void Activate()
    {
        Owner.Stat<Stat_Magic>().runes.Add(this);
        Owner.Trigger<Trigger_OnRuneUsed>(this, this);
    }
    public abstract void MagicEffect(Spell spell);
    public abstract void MagicEffectModifier(Spell spell);
    public abstract void Shape(Spell spell);
    public abstract void ShapeModifier(Spell spell);
}
