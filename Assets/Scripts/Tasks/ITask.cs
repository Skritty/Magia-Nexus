using System;

public interface ITask<Data>
{
    public bool DoTask(Data data) => false;
}

public interface ITaskOwned<Owner, Data> : ITask<Data>
{
    public bool DoTask(Owner owner, Data data) => false;
}

public class Task_Script<Data> : ITask<Data>
{
    protected Func<Data, bool> method;
    public Task_Script(Func<Data, bool> method) => this.method = method;
    public bool DoTask() => DoTask(default);
    public bool DoTask(Data data)
    {
        if (method == null) return false;
        return method.Invoke(data);
    }
}