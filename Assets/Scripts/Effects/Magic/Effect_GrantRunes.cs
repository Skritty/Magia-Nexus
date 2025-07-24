using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_GrantRunes<T> : EffectTask<T>
{
    public List<Rune> runes;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        foreach (Rune rune in runes)
        {
            Owner.GetMechanic<Mechanic_Magic>().AddRune(rune);
        }
    }
}
