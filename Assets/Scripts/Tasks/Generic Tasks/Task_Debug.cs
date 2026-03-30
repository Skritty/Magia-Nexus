using UnityEngine;

public class Task_Debug<T> : ITask<T>
{
    public bool DoTask(T data)
    {
        Debug.Log(data.ToString());
        return true;
    }
}
