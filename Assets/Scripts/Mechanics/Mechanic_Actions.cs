using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Stat_Stunned : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Channeling : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Actions : ListPrioritySolver<Action>, IStat<List<Action>> { }
public class Stat_Initiative : NumericalSolver, IStat<float> { }
public class Mechanic_Actions : Mechanic<Mechanic_Actions>
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
        if (tick < (TicksPerAction * actionsPerTurn) - Owner.Stat<Stat_Initiative>().Value)
        {
            Owner.AddModifier<bool, Stat_Stunned>(true, 1);
        }
        if (Owner.Stat<Stat_Stunned>().Value) return;
        FetchNextAction();
        DoCurrentAction();
    }

    /// <summary>
    /// Adds an action to the end of the action list, ignoring maximum phases
    /// </summary>
    public void AddAction(Action action)
    {
        CheckActionList();
        Owner.Stat<Stat_Actions>().Value.Add(action);
        actionsPerTurn = Owner.Stat<Stat_Actions>().Value.Count;
    }

    public void AddAction(Action action, int phase)
    {
        if (phase >= Owner.Stat<Stat_Actions>().Value.Count) actionsPerTurn = phase;
        CheckActionList();
        if (phase < 0 || phase >= Owner.Stat<Stat_Actions>().Value.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {Owner.Stat<Stat_Actions>().Value.Count})");
            return;
        }
        Owner.Stat<Stat_Actions>().Value[phase] = action;
    }

    public void SetAction(Action action, int phase)
    {
        CheckActionList();
        if(phase < 0 || phase >= Owner.Stat<Stat_Actions>().Value.Count)
        {
            Debug.LogWarning($"Add Action phase not in range (Adding {action.name} at phase {phase} / {Owner.Stat<Stat_Actions>().Value.Count})");
            return;
        }
        Owner.Stat<Stat_Actions>().Value[phase] = action;
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
        if(Owner.Stat<Stat_Actions>().Value == null)
        {
            Owner.Stat<Stat_Actions>().Add(actions);
        }
        if (actions.Count != actionsPerTurn)
        {
            actions.Clear();
            for (int i = 0; i < actionsPerTurn; i++)
            {
                if (Owner.Stat<Stat_Actions>().Value.Count > i)
                {
                    actions.Add(Owner.Stat<Stat_Actions>().Value[i]);
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
        List<Action> actions = Owner.Stat<Stat_Actions>().Value as List<Action>;
        if (actions.Count == 0) return;
        if (tick % TicksPerAction == 0)
        {
            currentAction?.OnEnd(Owner);
            if (Owner.Stat<Stat_Channeling>().Value)
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
}