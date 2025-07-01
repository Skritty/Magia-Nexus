using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Targeting : Mechanic<Stat_Targeting>
{
    [FoldoutGroup("Targeting"), SerializeReference]
    public Targeting targetingType = new Targeting_Distance();
    [FoldoutGroup("Targeting")]
    public int additionalTargets = 0;
}