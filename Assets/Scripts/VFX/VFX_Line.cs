using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix;
using UnityEngine;

public class VFX_Line : VFX_Damage
{
    public void ApplyLine(float length, float width)
    {
        visualEffect.SetFloat("Length", length);
        visualEffect.SetFloat("Width", width);
    }
}
