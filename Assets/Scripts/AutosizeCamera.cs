using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutosizeCamera : MonoBehaviour
{
    public Camera camera;
    public float buffer;

    private void OnPreRender()
    {
        Vector2 lowest = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 highest = new Vector2(float.MinValue, float.MinValue);
        foreach (Entity entity in Entity.FindObjectsOfType<Entity>()) // TODO: messy
        {
            if (!entity.Stat<Stat_PlayerOwner>().playerCharacter || !entity.gameObject.activeSelf) continue;
            if (entity.transform.position.x < lowest.x) lowest.x = entity.transform.position.x;
            if (entity.transform.position.y < lowest.y) lowest.y = entity.transform.position.y;
            if (entity.transform.position.x > highest.x) highest.x = entity.transform.position.x;
            if (entity.transform.position.y > highest.y) highest.y = entity.transform.position.y;
        }

        camera.transform.position = new Vector3((highest.x + lowest.x) / 2, (highest.y + lowest.y) / 2, -10);

        float size;
        lowest -= Vector2.one * buffer;
        highest += Vector2.one * buffer;
        if((highest.x - lowest.x) / Screen.width > (highest.y - lowest.y) / Screen.height)
        {
            size = (highest.x - lowest.x) / Screen.width;
        }
        else
        {
            size = (highest.y - lowest.y) / Screen.height;
        }
        camera.orthographicSize = size / 2 * Screen.height;

    }
}
