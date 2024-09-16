using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Charm : PersistentEffect
{
    private int originalTeam;
    public override void OnGained()
    {
        originalTeam = Target.Stat<Stat_Team>().team;
        Target.Stat<Stat_Team>().team = Owner.Stat<Stat_Team>().team;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Team>().team = originalTeam;
    }
}
