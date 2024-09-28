using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    [FoldoutGroup("@GetType()")]
    public EffectModifierType modifierType;
    [FoldoutGroup("@GetType()")]
    public EffectTag modifierTags;

    public override void OnGained()
    {
        Target.Stat<Stat_EffectModifiers>().AddMultiplier(this, modifierType, modifierTags, baseEffectMultiplier);
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveMultiplier(this);
    }
}
