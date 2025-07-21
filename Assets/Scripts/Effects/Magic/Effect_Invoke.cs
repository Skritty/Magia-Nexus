using System.Collections.Generic;

public class Effect_Invoke : EffectTask
{
    public bool useOwnerRunes;
    public List<Rune> runes = new List<Rune>();

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Spell spell;
        if (useOwnerRunes)
        {
            if (Owner.Stat<Stat_Runes>().Value.Count == 0) return;
            spell = new Spell(Owner, Owner.Stat<Stat_Runes>().Value);
        }
        else
        {
            if (runes.Count == 0) return;
            spell = new Spell(Owner, runes);
        }
        spell.GenerateSpell(this, null);
        Owner.GetMechanic<Mechanic_Magic>().ConsumeRunes();
    }
}