using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_GrantChanneledAction : PersistentEffect
{
    public Action channeledAction;

    public override void OnGained()
    {
        Target.Stat<Stat_Actions>().channeledAction = channeledAction;
    }

    public override void OnLost()
    {
        Target.Stat<Stat_Actions>().channeledAction = null;
    }
}
