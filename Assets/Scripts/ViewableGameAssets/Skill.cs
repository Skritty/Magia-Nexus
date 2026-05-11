using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ViewableGameAsset/Skill")]
public class Skill : ViewableGameAsset
{
    public AnimationState initialAnimationState = AnimationState.None;
    public AnimationState activateAnimationState = AnimationState.None;
    public int tickDuration;
    public DamageType tags;
    public AnimationCurve movementSpeedOverDuration;
    public float focusCost;
    public AnimationCurve focusExpenditureWeightOverTime;
    [ReadOnly]
    public float totalWeight;
    public float FocusCost(int tick)
    {
        return focusExpenditureWeightOverTime.Evaluate(tick * 1f / tickDuration) / totalWeight * focusCost;
    }
    
    public bool onTick;
    [Range(0,1), HideIf("@onTick")]
    public float timing = 0;
    public List<CancelRange> cancelRanges = new();
    public bool Cancelable(int tick, Skill other)
    {
        foreach(CancelRange cr in cancelRanges)
        {
            if (cr.range.x * tickDuration <= tick && 
                cr.range.y * tickDuration <= tick &&
                (((int)cr.cancelableBy) & ((int)other.tags)) != 0) 
                return true;
        }
        return false;
    }

    [Serializable]
    public class CancelRange
    {
        [MinMaxSlider(0, 1)]
        public Vector2 range;
        public DamageType cancelableBy = DamageType.All;
    }

    [SerializeReference]
    public List<EffectTask> effects = new List<EffectTask>();
    public virtual void OnStart(Entity owner)
    {
        owner.GetStat<Mechanic_AnimationStates>().AnimationState = initialAnimationState;
        Trigger_OnEntityStartSkill.Invoke((owner, this), this, owner);
        // TODO: return to this animation state after stunned
    }
    public virtual bool Tick(Entity owner, int tick)
    {
        owner.GetStat<Stat_MovementSpeed>().AddModifier(new Modifier_Numerical(value:movementSpeedOverDuration.Evaluate(tickDuration * 1f / tick), step: CalculationStep.Multiplicative, tickDuration: 1));
        if (onTick || tick == (int)(tickDuration * timing))
        {
            owner.GetStat<Mechanic_AnimationStates>().AnimationState = activateAnimationState;
            DoEffects(owner);
        }
        if(tick == tickDuration) return true;
        else return false;
    }
    public virtual void OnEnd(Entity owner)
    {
        Trigger_OnEntityEndSkill.Invoke((owner, this), this, owner);
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

    private void OnValidate()
    {
        tickDuration = Mathf.Clamp(tickDuration, 0, 500);
        totalWeight = 0;
        for(int i = 0; i < tickDuration; i++)
        {
            totalWeight += focusExpenditureWeightOverTime.Evaluate(i * 1f / tickDuration);
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
// Henshin, super punch, divekickk