/*using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Stat_Stunned : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Channeling : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Actions : ListPrioritySolver<Action>, IStat<List<Action>> { }
public class Stat_Initiative : NumericalSolver, IStat<float> { }
public class Mechanic_Actions : Mechanic
{
    [FoldoutGroup("Actions")]
    public int tick = -1;
    [FoldoutGroup("Actions")]
    public int actionsPerTurn;
    public float TimePerAction => GameManager.Instance.timePerTurn / actionsPerTurn;
    public int TicksPerAction => (int)(TimePerAction / Time.fixedDeltaTime);
    private int phase;
    [FoldoutGroup("Actions"), ShowInInspector]
    private Action currentAction;
    [FoldoutGroup("Actions")]
    public List<Action> actions = new List<Action>();

    public override void Tick()
    {
        tick++;
        if (tick < (TicksPerAction * actionsPerTurn) - Stats.GetStat<Stat_Initiative>(Owner).Value)
        {
            Stats.GetStat<Stat_Stunned>(Owner).AddModifier(true, 1);
        }
        if (Stats.GetStat<Stat_Stunned>(Owner).Value) return;
        FetchNextAction();
        DoCurrentAction();
    }

    /// <summary>
    /// Adds an action to the end of the action list, ignoring maximum phases
    /// </summary>
    public void AddAction(Action action)
    {
        CheckActionList();
        Stats.GetStat<Stat_Actions>(Owner).Value.Add(action);
        actionsPerTurn = Stats.GetStat<Stat_Actions>(Owner).Value.Count;
    }

    public void AddAction(Action action, int phase)
    {
        if (phase >= Stats.GetStat<Stat_Actions>(Owner).Value.Count) actionsPerTurn = phase;
        CheckActionList();
        if (phase < 0 || phase >= Stats.GetStat<Stat_Actions>(Owner).Value.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {Stats.GetStat<Stat_Actions>(Owner).Value.Count})");
            return;
        }
        Stats.GetStat<Stat_Actions>(Owner).Value[phase] = action;
    }

    public void SetAction(Action action, int phase)
    {
        CheckActionList();
        if(phase < 0 || phase >= Stats.GetStat<Stat_Actions>(Owner).Value.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {Stats.GetStat<Stat_Actions>(Owner).Value.Count})");
            return;
        }
        Stats.GetStat<Stat_Actions>(Owner).Value[phase] = action;
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
        if(Stats.GetStat<Stat_Actions>(Owner).Value == null)
        {
            Stats.GetStat<Stat_Actions>(Owner).Add(actions);
        }
        if (actions.Count != actionsPerTurn)
        {
            actions.Clear();
            for (int i = 0; i < actionsPerTurn; i++)
            {
                if (Stats.GetStat<Stat_Actions>(Owner).Value.Count > i)
                {
                    actions.Add(Stats.GetStat<Stat_Actions>(Owner).Value[i]);
                }
                else
                {
                    actions.Add(null);
                }
            }
        }
    }

    private void FetchNextAction()
    {
        List<Action> actions = Stats.GetStat<Stat_Actions>(Owner).Value;
        if (actions == null || actions.Count == 0) return;
        if (tick % TicksPerAction == 0)
        {
            currentAction?.OnEnd(Owner);
            if (Stats.GetStat<Stat_Channeling>(Owner).Value)
            {
                Trigger_Channel.Invoke(Owner, Owner);
            }
            else
            {
                phase++;
                if (phase - 1 == actions.Count)
                {
                    phase = 1;
                    Trigger_TurnEnd.Invoke(Owner, Owner);
                }

                currentAction = actions[phase - 1];

                if(phase == 1)
                {
                    Trigger_TurnStart.Invoke(Owner, Owner);
                }
            }
            currentAction?.OnStart(Owner);
            Trigger_ActionStart.Invoke(currentAction, Owner, currentAction);
        }
    }

    private void DoCurrentAction()
    {
        currentAction?.Tick(Owner, TicksPerAction, tick);
    }
}*/