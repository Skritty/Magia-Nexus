public interface IDataContainer 
{
    public bool Is<T>(out T data) where T : class, IDataContainer;
}

public interface IDataContainer<T> : IDataContainer
{
    public T Value { get; set; }
}

public interface IDataContainer_Float : IDataContainer
{
    public float Value { get; }
}

public interface IDataContainer_Boolean : IDataContainer
{
    public bool Value { get; }
}

public interface IDataContainer_Targeting : IDataContainer
{
    public Targeting Value { get; }
}

public interface IDataContainer_Player : IDataContainer
{
    public Viewer Player { get; }
}

public interface IDataContainer_OwnerEntity : IDataContainer
{
    public Entity Entity { get; }
}

public interface IDataContainer_Effect : IDataContainer
{
    public Effect Effect { get; }
}

public interface IDataContainer_PersistentEffect : IDataContainer
{
    public PersistentEffect PersistentEffect { get; }
}

public interface IDataContainer_DamageInstance : IDataContainer_Effect
{
    public DamageInstance Damage { get; }
}

public interface IDataContainer_Action : IDataContainer
{
    public Action Action { get; }
}

public interface IDataContainer_Rune : IDataContainer
{
    public Rune Rune { get; }
}

public interface IDataContainer_Spell : IDataContainer
{
    public Spell Spell { get; }
}