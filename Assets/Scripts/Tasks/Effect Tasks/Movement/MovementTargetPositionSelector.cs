using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public abstract class MovementTargetPositionSelector : EffectTask 
{
    [FoldoutGroup("@GetType()")]
    public float movementSpeedMultiplier = 1;

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        target.GetStat<Mechanic_Movement>().movementTargetPosition = GetMovementTargetPosition(owner, target, multiplier);
        target.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value: movementSpeedMultiplier * multiplier, step: CalculationStep.Multiplicative, tickDuration: 1));
    }

    protected abstract Vector3 GetMovementTargetPosition(Entity owner, Entity target, float multiplier);
}