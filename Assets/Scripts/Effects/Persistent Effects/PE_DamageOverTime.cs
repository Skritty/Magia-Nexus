using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PE_DamageOverTime : PersistentEffect
{
    [FoldoutGroup("@GetType()")]
    public DamageInstance damage;

    public override void OnTick()
    {
        damage.Create(this);
    }
}
