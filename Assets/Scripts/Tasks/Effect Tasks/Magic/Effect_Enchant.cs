using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_Enchant : EffectTask
{
    [FoldoutGroup("@GetType()")]
    public bool consumeRunes;
    [FoldoutGroup("@GetType()")]
    public int attacksEnchanted;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (Stats.GetStat<Stat_Runes>(Owner).Count == 0) return;
        for (int i = 0; i < attacksEnchanted; i++)
        {
            Stats.GetStat<Stat_Enchantments>(Owner).Enqueue(new List<Rune>(Stats.GetStat<Stat_Runes>(Owner)));
        }
        if (consumeRunes) Stats.GetStat<Stat_Runes>(Owner).Clear();
    }
}
