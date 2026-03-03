using UnityEngine;

public class Task_InstantiateAtPosition : ITask<MultidimensionalPosition>, ITask<Vector3>
{
    public GameObject obj;
    public bool DoTask(MultidimensionalPosition data)
    {
        GameObject.Instantiate(obj, data.ToVector3, Quaternion.identity);
        return true;
    }

    public bool DoTask(Vector3 data)
    {
        GameObject.Instantiate(obj, data, Quaternion.identity);
        return true;
    }
}
