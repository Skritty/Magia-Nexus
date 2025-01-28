public class Trigger_Action<T> : Trigger<T> where T : Trigger_Action<T> 
{
    public Action action;
    public Trigger_Action() { }
    protected Trigger_Action(Action action)
    {
        this.action = action;
        Invoke();
    }
}

public class Trigger_OnActionStart : Trigger_Action<Trigger_OnActionStart>
{
    public Trigger_OnActionStart() { }
    public Trigger_OnActionStart(Action action) : base(action) { }
}