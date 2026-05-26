using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_ChangeNumericalValueContainerStat : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public Modifier modifier;
    public Effect_ChangeNumericalValueContainerStat() { }
    public Effect_ChangeNumericalValueContainerStat(Modifier modifier)
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
        }
        modifier.AddToStatTag(target);
    }
}