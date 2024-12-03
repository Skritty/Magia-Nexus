using System;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix;
using UnityEngine;

public class Invoke : Effect
{
    public Entity spellPrefab;

    public override void Activate()
    {
        if (Owner.Stat<Stat_Magic>().runes.Count == 0) return;
        Spell spell = new Spell(Owner, Owner.Stat<Stat_Magic>().runes);
        spell.GenerateSpell(spellPrefab, null);
        Owner.Stat<Stat_Magic>().runes.Clear();
        spell.castSpell.Create(this);
        Owner.StartCoroutine(DestroyBlueprint(spell));
    }

    IEnumerator DestroyBlueprint(Spell spell)
    {
        for (int tick = 0; tick < Owner.Stat<Stat_Actions>().ticksPerPhase; tick++)
        {
            yield return new WaitForFixedUpdate();
        }
        GameObject.Destroy(spell.entity.gameObject);
    }
}