using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Invulnerable : PersistentEffect
{
    public override void OnGained()
    {
        target.Stat<Stat_Life>().invulnerable = true;
    }

    public override void OnLost()
    {
        target.Stat<Stat_Life>().invulnerable = false;
    }
}