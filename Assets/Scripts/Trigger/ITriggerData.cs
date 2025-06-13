public interface ITriggerData { }
public interface ITriggerData_Player : ITriggerData
{
    public Viewer Player { get; }
}

public interface ITriggerData_OwnerEntity : ITriggerData
{
    public Entity Entity { get; }
}

public interface ITriggerData_Effect : ITriggerData
{
    public Effect Effect { get; }
}

public interface ITriggerData_PersistentEffect : ITriggerData
{
    public PersistentEffect PersistentEffect { get; }
}

public interface ITriggerData_DamageInstance : ITriggerData_Effect
{
    public DamageInstance Damage { get; }
}

public interface ITriggerData_Action : ITriggerData
{
    public Action Action { get; }
}

public interface ITriggerData_Spell : ITriggerData
{
    public Spell Spell { get; }
}