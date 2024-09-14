using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_AdditionalPierces : Effect
{
    public int additionalPierces;
    public override void Activate()
    {
        target.Stat<Stat_Projectile>().piercesRemaining += additionalPierces;
    }
}