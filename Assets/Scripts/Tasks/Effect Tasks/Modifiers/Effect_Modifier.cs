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

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        if (multiplier != 1 && modifier is NumericalModifier)
        {
            NumericalModifier clone = (NumericalModifier)(modifier as Stat).Clone();
            Target.AddModifier(clone);
            clone.Step = CalculationStep.Multiplicative;
            clone.AddModifier(multiplier);
            return;
        }
        else
        {
            Target.AddModifier(modifier);
        }
    }
}