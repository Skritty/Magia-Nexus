public class TaskTrigger_Expire<T> : ITask<T>
{
    public Entity entity;
    public bool DoEffect(T data)
    {
        Trigger_Expire.Invoke(entity, entity, data);
        return true;
    }
}