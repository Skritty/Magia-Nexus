public class Trigger_ActionStart : Trigger<Trigger_ActionStart>, ITriggerData_OwnerEntity, ITriggerData_Action
{
    private Action _action;
    public Action Action => _action;
    private Entity _owner;
    public Entity Owner => _owner;

    public Trigger_ActionStart() { }
    public Trigger_ActionStart(Entity owner, Action action, params object[] bindingObjects)
    {
        _owner = owner;
        _action = action;
        Invoke(bindingObjects);
    }
}