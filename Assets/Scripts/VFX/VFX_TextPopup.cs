using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VFX_TextPopup : VFX
{
    public TextMeshProUGUI display;
    public float floatDistance;

    public void ApplyPopupInfo(string text, Color color)
    {
        display.text = text;
        display.color = color;
    }

    protected override void OnTick()
    {
        transform.position += new Vector3(0, floatDistance / tickDuration, 0);
    }
}
