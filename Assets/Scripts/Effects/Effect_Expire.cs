using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Expire<T> : EffectTask<T>
{
    [FoldoutGroup("@GetType()")]
    public bool disable;
    [FoldoutGroup("@GetType()")]
    public float delay;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
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