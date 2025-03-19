using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class VFX_AoE : VFX_Damage
{
    public void ApplyAoE(float radius, float angle)
    {
        visualEffect.SetFloat("Radius", radius);
        visualEffect.SetFloat("Angle", angle);
        visualEffect.SetFloat("Duration", 0.5f); // TODO: pass this in
    }
}
