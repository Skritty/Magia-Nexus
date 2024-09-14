using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_StatModifier : PersistentEffect
{
    [SerializeReference, Sirenix.OdinInspector.BoxGroup("Effect", showLabel: false)]
    public BaseStat statModifier;
    public override void OnGained()
    {
        base.OnGained();
    }

    public override void OnTick()
    {
        base.OnTick();
    }

    public override void OnLost()
    {
        base.OnLost();
    }
}
