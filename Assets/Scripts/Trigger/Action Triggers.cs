public class Trigger_ActionStart : Trigger<Action>
{
    public Trigger_ActionStart() { }
    public Trigger_ActionStart(Action action, params object[] bindingObjects)
    {
        Value = action;
        Invoke(bindingObjects);
    }
}

public class Trigger_ActionEnd : Trigger<Action>
{
    public Trigger_ActionEnd() { }
    public Trigger_ActionEnd(Action action, params object[] bindingObjects)
    {
        Value= action;
        Invoke(bindingObjects);
    }
}

public class Trigger_TurnStart : Trigger<Entity>
{
    public Trigger_TurnStart() { }
    public Trigger_TurnStart(Entity owner, params object[] bindingObjects)
    {
        Value = owner;
        Invoke(bindingObjects);
    }
}

public class Trigger_TurnEnd : Trigger<Entity>
{
    private Entity _owner;
    public Entity Entity => _owner;

    public Trigger_TurnEnd() { }
    public Trigger_TurnEnd(Entity owner, params object[] bindingObjects)
    {
        Value = owner;
        Invoke(bindingObjects);
    }
}