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

public class Trigger_TurnStart : Trigger<Trigger_TurnStart>, ITriggerData_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_TurnStart() { }
    public Trigger_TurnStart(Entity owner, params object[] bindingObjects)
    {
        _owner = owner;
        Invoke(bindingObjects);
    }
}

public class Trigger_TurnEnd : Trigger<Trigger_TurnEnd>, ITriggerData_OwnerEntity
{
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_TurnEnd() { }
    public Trigger_TurnEnd(Entity owner, params object[] bindingObjects)
    {
        _owner = owner;
        Invoke(bindingObjects);
    }
}