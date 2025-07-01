using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_GrantChanneledAction : PersistentEffect
{
    public Action channeledActions;

    public override void OnGained()
    {
        Target.GetMechanic<Stat_Actions>().channelInstead = true;
    }

    public override void OnLost()
    {
        Target.GetMechanic<Stat_Actions>().channelInstead = false;
    }
}
