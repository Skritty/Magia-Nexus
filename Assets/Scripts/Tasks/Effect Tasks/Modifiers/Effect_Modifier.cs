using System;
using Sirenix.OdinInspector;
using UnityEngine;

[LabelText("Task: Grant Modifier")]
public class Effect_Modifer : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public IModifier modifier;
    public Effect_Modifer() { }
    public Effect_Modifer(IModifier modifier)
    {
        this.modifier = modifier;
    }

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (modifier == null) return;
        if (modifier.Tag == null)
        {
            Debug.LogWarning($"{GetType()} of {owner.gameObject.name} is lacking a stat tag");
            return;
        }
        if (multiplier != 1 && modifier is Modifier_Numerical)
        {
            Modifier_Numerical clone = (modifier as Modifier_Numerical).Clone();
            clone.Value *= multiplier;
            target.AddModifier(clone);
        }
        else
        {
            target.TryAddModifier(modifier);
        }
    }
}