public interface ITriggerData { }
public interface ITriggerData_OwnerEntity : ITriggerData
{
    public Entity Owner { get; }
}

public interface ITriggerData_Effect : ITriggerData
{
    public Effect Effect { get; }
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