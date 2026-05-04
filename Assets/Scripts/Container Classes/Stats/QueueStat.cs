public class QueueStat<T> : CollectionContainer<T>
{
    public void Enqueue(T value)
    {
        AddModifier(new ValueContainer<T>(value));
    }

    public T Dequeue()
    {
        if (Modifiers.Count == 0) return default; 
        IValueContainer<T> modifier = Modifiers[0];
        Modifiers.RemoveAt(0);
        return modifier.Value;
    }

    public override void RemoveModifier(IValueContainer<T> modifier)
    {
        base.RemoveModifier(modifier);
    }
}