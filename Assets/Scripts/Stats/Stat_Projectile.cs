using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Projectile : GenericStat<Stat_Projectile>
{
    [FoldoutGroup("Projectile")]
    public int numberOfProjectiles = 1;
    [FoldoutGroup("Projectile")]
    public int piercesRemaining;
}