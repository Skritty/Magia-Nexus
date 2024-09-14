using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_Expire : Effect
{
    public float delay;
    public override void Activate()
    {
        Object.Destroy(target.gameObject, delay);
    }
}