using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

public class VFX_TextPopup : VFX
{
    public TextMeshProUGUI display;
    public float floatDistance;
    public Vector2 randomSpawnRange;
    private GraphicsBuffer textToSprite;

    public void ApplyPopupInfo(string text, Color color)
    {
        /*display.text = text;
        display.color = color;
        transform.position += new Vector3(
            Random.Range(-randomSpawnRange.x, randomSpawnRange.x),
            Random.Range(0, randomSpawnRange.y),
            0);*/
        textToSprite?.Dispose();
        textToSprite = new GraphicsBuffer(GraphicsBuffer.Target.Structured, text.Length, 4);
        float[] t2s = new float[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            t2s[i] = text[i] - 48;
        }
        textToSprite.SetData(t2s);
        visualEffect.SetGraphicsBuffer("Text", textToSprite);
        visualEffect.SetInt("TextLength", text.Length);
        visualEffect.SetFloat("Duration", tickDuration);
        //visualEffect.SetVector3("Offset");
    }

    protected override void OnTick()
    {
        transform.position += new Vector3(0, floatDistance / tickDuration, 0);
    }
}
