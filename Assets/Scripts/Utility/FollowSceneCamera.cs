using UnityEngine;

public class FollowSceneCamera : MonoBehaviour
{
    private void Update()
    {
        if (Camera.current == null) return;
        transform.position = Camera.current.transform.position;
        transform.rotation = Camera.current.transform.rotation;
    }
}
