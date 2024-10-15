using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_EffectModifer : PersistentEffect
{
    [FoldoutGroup("@GetType()")]
    public EffectModifierCalculationType modifierType;
    [FoldoutGroup("@GetType()")]
    public EffectTag modifierTags;

    public override void OnGained()
    {
        foreach (KeyValuePair<EffectTag, float> tag in effectTags)
        {
            Target.Stat<Stat_EffectModifiers>().AddModifier(this, modifierType, tag.Key, modifierTags, tag.Value);
        }
    }

    public override void OnLost()
    {
        Target.Stat<Stat_EffectModifiers>().RemoveModifier(this);
    }
}
