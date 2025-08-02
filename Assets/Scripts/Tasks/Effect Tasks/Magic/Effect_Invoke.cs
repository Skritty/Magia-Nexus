using System.Collections.Generic;

public class Effect_Invoke : EffectTask
{
    public bool useOwnerRunes;
    public List<Rune> runes = new();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Spell spell;
        if (useOwnerRunes)
        {
            if (Owner.Stat<Stat_Runes>().Count == 0) return;
            spell = new Spell(Owner, Owner.Stat<Stat_Runes>().ToList);
        }
        else
        {
            if (runes.Count == 0) return;
            spell = new Spell(Owner, runes);
        }
        spell.GenerateSpell(null);
        Owner.Stat<Stat_Runes>().Clear();
    }

    public new Effect_Invoke Clone()
    {
        Effect_Invoke clone = (Effect_Invoke)base.Clone();
        clone.runes = new List<Rune>(runes);
        return clone;
    }
}