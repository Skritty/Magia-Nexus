using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_Enchant<T> : EffectTask<T>
{
    [FoldoutGroup("@GetType()")]
    public bool consumeRunes;
    [FoldoutGroup("@GetType()")]
    public int attacksEnchanted;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (Owner.Stat<Stat_Runes>().Count == 0) return;
        for (int i = 0; i < attacksEnchanted; i++)
        {
            Owner.Stat<Stat_Enchantments>().Value.Enqueue(new List<Rune>(Owner.Stat<Stat_Runes>().ToArray));
        }
        if (consumeRunes) Owner.Stat<Stat_Runes>().Clear();
    }
}
