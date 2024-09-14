using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_GrantEntityEffects : Effect
{
    [SerializeReference]
    public List<Effect> effects = new();
    public override void Activate()
    {
        foreach (Effect effect in effects)
        {
            effect.Create(this);
        }
    }
}