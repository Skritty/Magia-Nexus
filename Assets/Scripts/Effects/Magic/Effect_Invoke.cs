using System.Collections.Generic;

public class Effect_Invoke<T> : EffectTask<T>
{
    public bool useOwnerRunes;
    public Rune[] runes = new Rune[0];

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Spell spell;
        if (useOwnerRunes)
        {
            if (Owner.Stat<Stat_Runes>().Count == 0) return;
            spell = new Spell(Owner, Owner.Stat<Stat_Runes>().ToArray);
        }
        else
        {
            if (runes.Length == 0) return;
            spell = new Spell(Owner, runes);
        }
        spell.GenerateSpell(null);
        Owner.Stat<Stat_Runes>().Clear();
    }

    public new Effect_Invoke<T> Clone()
    {
        Effect_Invoke<T> clone = (Effect_Invoke<T>)base.Clone();
        clone.runes = new Rune[runes.Length];
        return clone;
    }
}