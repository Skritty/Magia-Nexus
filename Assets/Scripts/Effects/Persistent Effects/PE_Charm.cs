using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_Charm : PersistentEffect
{
    private int originalTeam;
    public override void OnGained()
    {
        originalTeam = Target.GetMechanic<Stat_Team>().team;
        Target.GetMechanic<Stat_Team>().team = Owner.GetMechanic<Stat_Team>().team;
    }

    public override void OnLost()
    {
        Target.GetMechanic<Stat_Team>().team = originalTeam;
    }
}
