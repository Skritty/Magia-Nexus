using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rune : Effect
{
    public Action opposite;

    public override void Activate()
    {
        owner.Stat<Stat_Magic>().runes.Add(this);
        owner.Trigger<Trigger_OnRuneUsed>(this);
    }
    public abstract void SpellEffect(Spell spell);
    public abstract Rune EffectFormula(Spell spell, Rune combiningRune);
    public virtual void FinalizeEffectFormula(Spell spell) { }
    public abstract Rune TargetingFormula(Spell spell, Rune combiningRune);
}
