using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_LifeBuff : PersistentEffect
{
    public float maxLifeChange;
    public float currentLifeChange;
    public override void OnGained()
    {
        Target.Stat<Stat_Life>().maxLife += maxLifeChange;
        Target.Stat<Stat_Life>().currentLife += currentLifeChange;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Life>().maxLife -= maxLifeChange;
    }
}