using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ViewableGameAsset/Action")]
public class Action : ViewableGameAsset
{
    public int maxUses;
    public AnimationState initialAnimationState = AnimationState.None;
    public AnimationState activateAnimationState = AnimationState.None;
    public AnimationCurve movementSpeedOverDuration;
    public bool onTick;
    [Range(0,1)]
    public float timing = 0;
    public float effectMultiplier = 1;
    [SerializeReference]
    public List<EffectTask> effects = new List<EffectTask>();
    public virtual void OnStart(Entity owner)
    {
        owner.GetMechanic<Mechanic_AnimationStates>().AnimationState = initialAnimationState;
        // TODO: return to this animation state after stunned
    }
    public virtual void Tick(Entity owner, int tickLength, int tick)
    {
        owner.AddModifier<float, Stat_MovementSpeed>(new Modifier_Numerical(value:movementSpeedOverDuration.Evaluate((tick % tickLength) * 1f / tickLength), step: CalculationStep.Multiplicative, tickDuration: 1));
        if (onTick || tick % tickLength == (int)(tickLength * timing))
        {
            owner.GetMechanic<Mechanic_AnimationStates>().AnimationState = activateAnimationState;
            DoEffects(owner);
        }
    }
    public virtual void OnEnd(Entity owner)
    {

    }
    public void DoEffects(Entity owner)
    {
        foreach (EffectTask effect in effects)
        {
            if(effect == null)
            {
                Debug.LogWarning($"{name} has null effects!");
                continue;
            }
            effect.DoTask(owner);
        }
    }
}

//          GAMEPLAY MECHANICS
// Turns go by at the same rate, ie. 1 turn = 2.5s
// Players have a # of actions per turn (base 5 or something)
// Items could give players more or less actions per turn.
// Turn Example 1: approach, attack, dodge, attack, backstep (1s/action)
// Turn Example 2: approach, parry, attack (1.67s/action)
// Turn Example 3: approach, backstep, approach, attack, attack, approach, attack (0.625s/action)
// Turn Example 4: wind, wind, cast, fire, fire

// Basic Actions: Approach, Retreat, Attack, [Spellcasting Elements]
// Limited Action: Parry, Block, Backstep, Cast, Continue
// !createTurn approach, parry, attack

// Spellcasting Elements:
// Earth, Wind, Fire, Water, Chaos, Order

// Henshin, slash, move
// Henshin, super punch, divekick