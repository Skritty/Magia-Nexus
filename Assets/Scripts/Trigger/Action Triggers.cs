public class Trigger_ActionStart : Trigger<Trigger_ActionStart>, ITriggerData_OwnerEntity, ITriggerData_Action
{
    private Action _action;
    public Action Action => _action;
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_ActionStart() { }
    public Trigger_ActionStart(Entity owner, Action action, params object[] bindingObjects)
    {
        _owner = owner;
        _action = action;
        Invoke(bindingObjects);
    }
}

public class Trigger_ActionEnd : Trigger<Trigger_ActionEnd>, ITriggerData_OwnerEntity, ITriggerData_Action
{
    private Action _action;
    public Action Action => _action;
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_ActionEnd() { }
    public Trigger_ActionEnd(Entity owner, Action action, params object[] bindingObjects)
    {
        _owner = owner;
        _action = action;
        Invoke(bindingObjects);
    }
}

public class Trigger_TurnComplete : Trigger<Trigger_TurnComplete>, ITriggerData_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_TurnComplete() { }
    public Trigger_TurnComplete(Entity owner, params object[] bindingObjects)
    {
        _owner = owner;
        Invoke(bindingObjects);
    }
}