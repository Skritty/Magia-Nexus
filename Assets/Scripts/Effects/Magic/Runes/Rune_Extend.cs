using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Extend : Rune
{
    public override void MagicEffect(DamageInstance damage)
    {
        // Spell phase +1
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        return;
    }

    public override void Shape(Spell spell)
    {
        // Shape?
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        //new Spell();
    }
}
