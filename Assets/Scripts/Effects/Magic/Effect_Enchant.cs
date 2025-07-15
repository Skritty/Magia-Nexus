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
        if (Owner.GetMechanic<Mechanic_Magic>().runes.Count == 0) return;
        for (int i = 0; i < attacksEnchanted; i++)
        {
            Owner.GetMechanic<Mechanic_Magic>().enchantedAttacks.Enqueue(new List<Rune>(Owner.GetMechanic<Mechanic_Magic>().runes));
        }
        if (consumeRunes) Owner.GetMechanic<Mechanic_Magic>().ConsumeRunes();
    }
}
