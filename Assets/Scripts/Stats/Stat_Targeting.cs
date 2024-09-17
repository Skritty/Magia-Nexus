using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Targeting : GenericStat<Stat_Targeting>
{
    [FoldoutGroup("Targeting"), SerializeReference]
    public Targeting targetingType = new Targeting_Distance();
    [FoldoutGroup("Targeting")]
    public int numberOfTargets = 1;
}