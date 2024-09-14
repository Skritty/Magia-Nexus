using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_ExpireEntity : PersistentEffect
{
    public override void OnLost()
    {
        //owner.ReleaseObject();
        Object.Destroy(owner.gameObject);
    }
}