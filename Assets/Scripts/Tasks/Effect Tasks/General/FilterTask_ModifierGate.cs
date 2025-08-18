using Sirenix.OdinInspector;
using UnityEngine;

[LabelText("Filter: Contains X of modifier on stat")]
public class FilterTask_ModifierGate : EffectTask
{
    [SerializeReference]
    public IModifier modifier;

    public float requiredAmount;
    public int stacksAddedOnSuccess;
    public int stacksAddedOnFailure;
    public bool invert;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered) { }

    public override bool DoTask(Entity owner, Entity target, Effect data, bool triggered, Entity proxy = null)
    {
        IStat stat = owner.Stat(modifier.Tag);
        if (stat != null && stat.Contains(modifier, out int count) && count >= requiredAmount)
        {
            if (stacksAddedOnSuccess != 0)
            {
                for (int i = 0; i < Mathf.Abs(stacksAddedOnSuccess); i++)
                {
                    if (stacksAddedOnSuccess > 0)
                    {
                        stat.TryAdd(modifier);
                    }
                    else
                    {
                        stat.Remove(modifier);
                    }
                }
            }
            return true & !invert;
        }
        else
        {
            if (stacksAddedOnFailure != 0)
            {
                for (int i = 0; i < Mathf.Abs(stacksAddedOnFailure); i++)
                {
                    if (stacksAddedOnFailure > 0)
                    {
                        stat.TryAdd(modifier);
                    }
                    else
                    {
                        stat.Remove(modifier);
                    }
                }
            }
            return false | invert;
        }
    }
}