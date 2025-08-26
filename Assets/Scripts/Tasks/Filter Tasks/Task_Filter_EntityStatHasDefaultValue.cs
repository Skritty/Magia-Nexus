using UnityEngine;

public class Task_Filter_EntityStatHasDefaultValue<T> : ITask<Entity>
{
    public bool invert;
    [SerializeReference]
    public IModifier statReferenceModifier;
    public bool DoTask(Entity data)
    {
        if (statReferenceModifier == null || statReferenceModifier.Tag == null) return true & !invert;
        if ((data.Stat(statReferenceModifier.Tag) as IDataContainer).IsDefaultValue()) return true & !invert;
        return false | invert;
    }
}
