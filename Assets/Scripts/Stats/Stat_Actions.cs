using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Stat_Actions : GenericStat<Stat_Actions>
{

    [FoldoutGroup("Actions")]
    [Sirenix.OdinInspector.ReadOnly]
    public int tick;
    [FoldoutGroup("Actions")]
    public bool stunned;
    [FoldoutGroup("Actions")]
    public int actionsPerTurn;
    [FoldoutGroup("Actions")]
    public List<Action> repeatActions = new List<Action>(); // Happen before each action (Only do OnStart)
    [FoldoutGroup("Actions")]
    public List<Action> actions = new List<Action>();

    private int ticksPerPhase => (int)(GameManager.Instance.timePerTurn / actionsPerTurn * (1 / Time.fixedDeltaTime));
    private int phase;
    private Action currentAction;

    public override void Tick()
    {
        tick++;
        FetchNextAction();
        DoCurrentAction();
    }

    /// <summary>
    /// Adds an action to the end of the action list, ignoring maximum phases
    /// </summary>
    public void AddAction(Action action)
    {
        actions.Add(action);
    }

    public void AddAction(Action action, int phase)
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

    private void CheckActionList()
    {
        if (actions.Count != actionsPerTurn)
        {
            List<Action> newList = new List<Action>();
            for (int i = 0; i < actionsPerTurn; i++)
            {
                if (actions.Count > i)
                {
                    newList.Add(actions[i]);
                }
                else
                {
                    newList.Add(null);
                }
            }
            actions = newList;
        }
    }

    private void FetchNextAction()
    {
        if (actions.Count == 0) return;
        if (tick % ticksPerPhase == 0)
        {
            phase++;
            if (phase - 1 == actions.Count) phase = 1;
            currentAction?.OnEnd(owner);
            currentAction = actions[phase - 1];
            foreach (Action a in repeatActions)
            {
                if(tick != 0)
                    a?.OnEnd(owner);
                a?.OnStart(owner);
                owner.Trigger<Trigger_OnActionStart>(this);
            }
            currentAction?.OnStart(owner);
        }
    }

    private void DoCurrentAction()
    {
        foreach (Action a in repeatActions)
        {
            a?.Tick(owner);
        }
        currentAction?.Tick(owner);
    }
}