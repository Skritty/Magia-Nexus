using Sirenix.OdinInspector;
using UnityEngine;

public class Task_Expire<T> : ITaskOwned<Entity, T>
{
    [FoldoutGroup("@GetType()")]
    public bool disable;
    [FoldoutGroup("@GetType()")]
    public float delay;

    public bool DoTask(T data) => false;
    public bool DoTask(Entity owner, T data)
    {
        if (disable)
        {
            owner.gameObject.SetActive(false);
        }
        else
        {
            Object.Destroy(owner.gameObject, delay);
        }
        return true;
    }
}