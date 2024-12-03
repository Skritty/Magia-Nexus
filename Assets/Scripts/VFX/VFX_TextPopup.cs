using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VFX_TextPopup : VFX
{
    public TextMeshProUGUI display;
    public float floatDistance;
    public Vector2 randomSpawnRange;

    public void ApplyPopupInfo(string text, Color color)
    {
        display.text = text;
        display.color = color;
        transform.position += new Vector3(
            Random.Range(-randomSpawnRange.x, randomSpawnRange.x),
            Random.Range(0, randomSpawnRange.y),
            0);
    }

    protected override void OnTick()
    {
        transform.position += new Vector3(0, floatDistance / tickDuration, 0);
    }
}
