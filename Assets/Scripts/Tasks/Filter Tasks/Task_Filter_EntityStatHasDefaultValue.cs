using UnityEngine;

public class Task_Filter_EntityStatHasDefaultValue<T> : ITask<Entity>
{
    public bool invert;
    [SerializeReference]
    public Modifier statReferenceModifier;
    public bool DoTask(Entity data)
    {
        if (statReferenceModifier == null || statReferenceModifier.Tag == null) return true & !invert;
        if ((data.GetStat(statReferenceModifier.Tag) as IValueContainer).IsDefaultValue()) return true & !invert;
        return false | invert;
    }
}
