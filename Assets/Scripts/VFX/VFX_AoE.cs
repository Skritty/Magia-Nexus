using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class VFX_AoE : VFX_Damage
{
    public void ApplyAoE(float radius, float angle, float duration)
    {
        visualEffect.SetFloat("Radius", radius);
        visualEffect.SetFloat("Angle", angle);
        visualEffect.SetFloat("Duration", duration);
    }
}
