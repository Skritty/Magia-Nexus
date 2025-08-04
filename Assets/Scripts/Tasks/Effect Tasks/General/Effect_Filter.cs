/*using System.Collections.Generic;

public abstract class Effect_Filter<T> : EffectTask
{
    public List<ITask<T>> tasks;
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered) { }
    public override bool DoTask(Entity owner, Entity target, Effect data, bool triggered, Entity proxy = null)
    {
        foreach (ITask<T> task in tasks)
        {
            task.DoTask(data);
        }
    }
}
*/