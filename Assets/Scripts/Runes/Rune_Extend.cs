using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Extend")]
public class Rune_Extend : Rune
{
    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        // Spell phase +1
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        List<Rune> runes = new List<Rune>();
        for(int i = 1; i < damage.runes.Count; i++)
        {
            runes.Add(damage.runes[(i + currentRuneIndex) % damage.runes.Count]);
        }

        Effect_Invoke invoke = new Effect_Invoke();
        invoke.runes = runes;

        //damage.postOnHitEffects.Add(invoke);// TODO: Proxy from hit target
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        if (spell.runes.Count(x => x is Rune_Extend) == spell.runes.Count) return;
        int index = (currentRuneIndex + 1) % spell.runes.Count;
        spell.runes[index].Shape(spell, index);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {

    }
}
