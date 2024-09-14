using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Charm : PersistentEffect
{
    private int originalTeam;
    public override void OnGained()
    {
        originalTeam = target.Stat<Stat_Team>().team;
        target.Stat<Stat_Team>().team = owner.Stat<Stat_Team>().team;
    }

    public override void OnLost()
    {
        target.Stat<Stat_Team>().team = originalTeam;
    }
}
