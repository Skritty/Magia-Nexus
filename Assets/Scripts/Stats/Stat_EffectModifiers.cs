using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_EffectModifiers : GenericStat<Stat_EffectModifiers>
{
    [FoldoutGroup("Effect Modifiers")]
    public float effectMultiplier = 1;
    [FoldoutGroup("Effect Modifiers")]
    public float aoeMultiplier = 1;
}