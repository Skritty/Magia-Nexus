using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expire : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool disable;
    [FoldoutGroup("@GetType()")]
    public float delay;
    public override void Activate()
    {
        if (disable)
        {
            Target.gameObject.SetActive(false);
        }
        else
        {
            Object.Destroy(Target.gameObject, delay);
        }
    }
}