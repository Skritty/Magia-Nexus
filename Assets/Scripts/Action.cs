using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ActionEventTiming
{
    OnStart = 1,
    OnTick = 2,
    OnEnd = 4
}

public enum ActionType { Basic, Targeting, Movement }

[Serializable, CreateAssetMenu(menuName = "Action")]
public class Action : ScriptableObject
{
    public string actionName;
    [TextArea]
    public string actionDescription;
    public Sprite actionImage;
    public Color damageTypeColor = Color.white;
    public int maxUses;
    public ActionType type;
    public ActionEventTiming timing = ActionEventTiming.OnStart;
    [SerializeReference]
    public List<Effect> effects = new List<Effect>();
    public virtual void OnStart(Entity owner) 
    {
        if (timing.HasFlag(ActionEventTiming.OnStart))
        {
            foreach(Effect effect in effects)
            {
                effect.Create(this, owner);
            }
        }
    }
    public virtual void Tick(Entity owner)
    {
        if (timing.HasFlag(ActionEventTiming.OnTick))
        {
            foreach (Effect effect in effects)
            {
                effect.Create(this, owner);
            }
        }
    }
    public virtual void OnEnd(Entity owner)
    {
        if (timing.HasFlag(ActionEventTiming.OnEnd))
        {
            foreach (Effect effect in effects)
            {
                effect.Create(this, owner);
            }
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