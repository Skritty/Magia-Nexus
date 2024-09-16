using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expire : Effect
{
    public float delay;
    public override void Activate()
    {
        Object.Destroy(Target.gameObject, delay);
    }
}