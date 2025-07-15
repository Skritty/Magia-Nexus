using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutosizeCamera : MonoBehaviour
{
    public new Camera camera;
    public float buffer;
    public float positionLerpSpeed = 0.5f;
    public float sizeLerpSpeed = 0.5f;

    private Vector3 targetPosition = Vector3.zero;
    private float targetSize = 0;

    private void Update()
    {
        Vector2 lowest = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 highest = new Vector2(float.MinValue, float.MinValue);
        foreach (Entity entity in Entity.FindObjectsOfType<Entity>()) // TODO: messy
        {
            if (!entity.GetMechanic<Mechanic_PlayerOwner>().playerCharacter || !entity.gameObject.activeSelf) continue;
            if (entity.transform.position.x < lowest.x) lowest.x = entity.transform.position.x;
            if (entity.transform.position.y < lowest.y) lowest.y = entity.transform.position.y;
            if (entity.transform.position.x > highest.x) highest.x = entity.transform.position.x;
            if (entity.transform.position.y > highest.y) highest.y = entity.transform.position.y;
        }
        targetPosition = new Vector3((highest.x + lowest.x) / 2, (highest.y + lowest.y) / 2, -10);

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
        targetSize = size / 2 * Screen.height;

        if (targetPosition != Vector3.zero)
            camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, positionLerpSpeed);
        if (targetSize != 0) 
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, sizeLerpSpeed);
    }
}
