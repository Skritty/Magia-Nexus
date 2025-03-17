using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Action")]
public class Action : ScriptableObject
{
    public List<string> actionAliases = new List<string>();
    public string ActionName => actionAliases.Count == 0 ? "" : actionAliases[0];
    public bool NameMatch(string s)
    {
        s = s.ToLower().Replace(" ", "");
        foreach (string name in actionAliases)
        {
            if (name.ToLower().Replace(" ", "").Equals(s)) return true;
        }
        return false;
    }
    [TextArea]
    public string actionDescription;
    public Sprite actionImage;
    public Color damageTypeColor = Color.white;
    public bool hidden;
    public int maxUses;
    public AnimationState initialAnimationState;
    public AnimationState activateAnimationState;
    public AnimationCurve movementSpeedOverDuration;
    public bool onTick;
    [Range(0,1)]
    public float timing = 0;
    public float effectMultiplier = 1;
    [SerializeReference]
    public List<Effect> effects = new List<Effect>();
    public virtual void OnStart(Entity owner) 
    {
        owner.Stat<Stat_AnimationStates>().AnimationState = initialAnimationState;
        // TODO: return to this animation state after stunned
    }
    public virtual void Tick(Entity owner, int tickLength, int tick)
    {
        owner.Stat<Stat_Movement>().dirMovementSpeedMulti = movementSpeedOverDuration.Evaluate((tick % tickLength) * 1f / tickLength);
        if (onTick || tick % tickLength == (int)(tickLength * timing))
        {
            owner.Stat<Stat_AnimationStates>().AnimationState = activateAnimationState;
            DoEffects(owner);
        }
    }
    public virtual void OnEnd(Entity owner)
    {
        owner.Stat<Stat_AnimationStates>().AnimationState = AnimationState.Move;
    }
    public void DoEffects(Entity owner)
    {
        foreach (Effect effect in effects)
        {
            effect.Create(owner, effectMultiplier);
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