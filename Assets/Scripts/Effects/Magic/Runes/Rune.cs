using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : Effect
{
    public EffectTag damageTags;
    public Action opposite;
    protected static string TypeName(Rune rune) => rune == null ? "" : rune.GetType().Name;

    public override void Activate()
    {
        Owner.Stat<Stat_Magic>().runes.Add(this);
        Owner.Trigger<Trigger_OnRuneUsed>(this, this);
    }
    public abstract void SpellModifier(Spell spell);
    public abstract Rune EffectFormula(Spell spell, Rune combiningRune);
    public virtual void FinalizeEffectFormula(Spell spell) { }
    public abstract Rune TargetingFormula(Spell spell, Rune combiningRune);
    public virtual void FinalizeTargetingFormula(Spell spell) { }
}
