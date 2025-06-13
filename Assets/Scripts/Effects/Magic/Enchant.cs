using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Enchant : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool consumeRunes;
    [FoldoutGroup("@GetType()")]
    public int attacksEnchanted;
    public override void Activate()
    {
        if (Owner.Stat<Stat_Magic>().runes.Count == 0) return;
        for(int i = 0; i < attacksEnchanted; i++)
        {
            Owner.Stat<Stat_Magic>().enchantedAttacks.Enqueue(new List<Rune>(Owner.Stat<Stat_Magic>().runes));
        }
        if(consumeRunes) Owner.Stat<Stat_Magic>().ConsumeRunes();
    }
}
