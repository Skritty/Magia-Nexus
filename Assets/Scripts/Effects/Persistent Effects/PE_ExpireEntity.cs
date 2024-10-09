using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_ExpireEntity : PersistentEffect
{
    public override void OnLost()
    {
        //if (Owner == null) return;
        //owner.ReleaseObject();
        Object.Destroy(Owner.gameObject);
    }
}