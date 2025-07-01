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
        if (Owner.GetMechanic<Stat_Magic>().runes.Count == 0) return;
        for(int i = 0; i < attacksEnchanted; i++)
        {
            Owner.GetMechanic<Stat_Magic>().enchantedAttacks.Enqueue(new List<Rune>(Owner.GetMechanic<Stat_Magic>().runes));
        }
        if(consumeRunes) Owner.GetMechanic<Stat_Magic>().ConsumeRunes();
    }
}
