using System.Collections.Generic;

public class Effect_Invoke : EffectTask
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
        spell.GenerateSpell(this, null);
        Owner.Stat<Stat_Runes>().Clear();
    }

    public new void Clone()
    {
        Effect_Invoke clone = (Effect_Invoke)base.Clone();
        clone.runes = new Rune[runes.Length];
    }
}