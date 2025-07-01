using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Stat_Actions : Mechanic<Stat_Actions>
{

    [FoldoutGroup("Actions")]
    public int tick = -1;
    [FoldoutGroup("Actions")]
    public bool stunned;
    [FoldoutGroup("Actions")]
    public int actionsPerTurn;
    [FoldoutGroup("Actions")]
    public int startingTickDelay;
    [FoldoutGroup("Actions")]
    public List<Action> repeatActions = new List<Action>(); // Happen before each action (Only do OnStart)
    [FoldoutGroup("Actions")]
    public List<Action> actions = new List<Action>();
    [FoldoutGroup("Actions")]
    public List<Action> actionsOverride = new List<Action>();
    [FoldoutGroup("Actions")]
    public bool channelInstead;

    public float TimePerAction => GameManager.Instance.timePerTurn / actionsPerTurn;
    public int TicksPerAction => (int)(TimePerAction / Time.fixedDeltaTime);
    private int phase;
    [ShowInInspector]
    private Action currentAction;

    public override void Tick()
    {
        if (!stunned)
        {
            tick++;
        }
        FetchNextAction();
        DoCurrentAction();
    }

    /// <summary>
    /// Adds an action to the end of the action list, ignoring maximum phases
    /// </summary>
    public void AddAction(Action action)
    {
        actions.Add(action);
        actionsOverride.Add(null);
        actionsPerTurn = actions.Count;
    }

    public void AddAction(Action action, int phase)
    {
        if (phase >= actions.Count) actionsPerTurn = phase;
        CheckActionList();
        if (phase < 0 || phase >= actions.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {actions.Count})");
            return;
        }
        actions[phase] = action;
    }

    public void SetAction(Action action, int phase)
    {
        CheckActionList();
        if(phase < 0 || phase >= actions.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {actions.Count})");
            return;
        }
        actions[phase] = action;
    }

    public void AddRepeatAction(Action action)
    {
        repeatActions.Add(action);
    }

    public void RemoveAction(Action action)
    {
        CheckActionList();
    }

    public void RemovePhase()
    {
        CheckActionList();
    }

    public void OverrideActions(SerializedDictionary<Action, Action> overrides, bool clear = false)
    {
        for(int i = 0; i < actions.Count; i++)
        {
            if (overrides.ContainsKey(actions[i]))
            {
                if (clear)
                {
                    actionsOverride[i] = null;
                }
                else
                {
                    actionsOverride[i] = overrides[actions[i]];
                }
            }
        }
    }

    private void CheckActionList()
    {
        if (actions.Count != actionsPerTurn)
        {
            List<Action> newList = new List<Action>();
            List<Action> newOverrideList = new List<Action>();
            for (int i = 0; i < actionsPerTurn; i++)
            {
                if (actions.Count > i)
                {
                    newList.Add(actions[i]);
                    newOverrideList.Add(actionsOverride[i]);
                }
                else
                {
                    newList.Add(null);
                    newOverrideList.Add(null);
                }
            }
            actions = newList;
            actionsOverride = newOverrideList;
        }
    }

    private void FetchNextAction()
    {
        if (actions.Count == 0 || tick < startingTickDelay - Owner.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.Initiative)) return;
        if (tick % TicksPerAction == 0)
        {
            currentAction?.OnEnd(Owner);
            if (channelInstead)
            {
                new Trigger_Channel(Owner, Owner);
            }
            else
            {
                phase++;
                if (phase - 1 == actions.Count)
                {
                    phase = 1;
                    new Trigger_TurnEnd(Owner, Owner);
                }
                if(actionsOverride[phase - 1])
                {
                    currentAction = actionsOverride[phase - 1];
                }
                else
                {
                    currentAction = actions[phase - 1];
                }
                if(phase == 1)
                {
                    new Trigger_TurnStart(Owner, Owner);
                }
            }
            
            foreach (Action a in repeatActions)
            {
                if(tick != 0)
                    a?.OnEnd(Owner);
                a?.OnStart(Owner);
            }
            currentAction?.OnStart(Owner);
            new Trigger_ActionStart(Owner, currentAction, Owner, currentAction);
        }
    }

    private void DoCurrentAction()
    {
        if (stunned) return;
        foreach (Action a in repeatActions)
        {
            a?.Tick(Owner, TicksPerAction, tick);
        }
        currentAction?.Tick(Owner, TicksPerAction, tick);
    }
}