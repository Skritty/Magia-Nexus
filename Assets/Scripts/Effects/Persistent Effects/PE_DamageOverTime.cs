using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_DamageOverTime : PersistentEffect
{
    public DamageInstance damage;

    public override void OnTick()
    {
        damage.Create(this);
    }
}
