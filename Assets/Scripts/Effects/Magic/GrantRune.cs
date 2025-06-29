using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrantRunes : Effect
{
    public List<Rune> runes;
    public override void Activate()
    {
        foreach (Rune rune in runes)
        {
            Owner.Stat<Stat_Magic>().AddRune(rune);
        }
    }
}
