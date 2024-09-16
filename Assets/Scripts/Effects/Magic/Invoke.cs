using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invoke : Effect
{
    public Entity spellPrefab;
    public Action spellAction;

    public override void Activate()
    {
        List<Spell> spells = new List<Spell>();
        PreassembleSpells(spells, null, Owner.Stat<Stat_Magic>().runes, 0);

        Spell triggeredSpell = null;
        for (int i = spells.Count - 1; 0 <= i; i--)
        {
            spells[i].GenerateSpell(spellPrefab, spellAction, triggeredSpell);
            triggeredSpell = spells[i];
        }

        Owner.Stat<Stat_Magic>().runes.Clear();
        spells[0].castSpell.Create(this);
        foreach (Spell spell in spells)
        {
            GameObject.Destroy(spell.entity.gameObject);
        }
    }

    private void PreassembleSpells(List<Spell> spells, Spell spell, List<Rune> runes, int index)
    {
        if (index == runes.Count) return;
        if (index == 0)
        {
            spell = new Spell(runes[index], runes);
            spell.owner = Owner;
            spells.Add(spell);
            PreassembleSpells(spells, spell, runes, ++index);
            return;
        }
        //TODO: extend rune
        if(index % 2 == 0)
        {
            spell.spellModifiers.Add(runes[index]);
            PreassembleSpells(spells, spell, runes, ++index);
            return;
        }
        else
        {
            spell.shapeModifiers.Add(runes[index]);
            PreassembleSpells(spells, spell, runes, ++index);
            return;
        }
    }
}