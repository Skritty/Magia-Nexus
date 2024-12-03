using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Extend : Rune
{
    public override void MagicEffect(Spell spell)
    {
        // Spell phase +1
    }

    public override void MagicEffectModifier(Spell spell)
    {
        return;
    }

    public override void Shape(Spell spell)
    {
        // Shape?
    }

    public override void ShapeModifier(Spell spell)
    {
        //new Spell();
    }
}
