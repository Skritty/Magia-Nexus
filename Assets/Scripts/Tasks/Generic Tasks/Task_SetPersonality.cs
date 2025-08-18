using UnityEngine;

public class Task_SetPersonality<T> : ITaskOwned<Entity, T>
{
    public Personality personality;
    public bool DoTask(Entity owner, T data)
    {
        personality.SetPersonality(owner);
        return true;
    }

    public bool DoTask(T data)
    {
        return false;
    }
}
