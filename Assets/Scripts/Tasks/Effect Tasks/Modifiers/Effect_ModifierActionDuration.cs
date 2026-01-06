using Sirenix.OdinInspector;
using UnityEngine;


[LabelText("Task: Grant Modifier with action duration")]
public class Effect_ModifierActionDuration : EffectTask
{
    [SerializeReference, FoldoutGroup("@GetType()")]
    public IModifier modifier;
    public Effect_ModifierActionDuration() { }
    public Effect_ModifierActionDuration(IModifier modifier)
    {
        this.modifier = modifier;
    }
    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        if (multiplier != 1 && modifier is Modifier_Numerical)
        {
            Modifier_Numerical clone = (modifier as Modifier_Numerical).Clone();
            target.AddModifier(clone, 25);// owner.GetMechanic<Mechanic_Actions>().TicksPerAction);
            clone.Step = CalculationStep.Multiplicative;
            clone.Value *= multiplier;
            return;
        }
        else
        {
            target.TryAddModifier(modifier, 25);// owner.GetMechanic<Mechanic_Actions>().TicksPerAction);
        }
    }
}
