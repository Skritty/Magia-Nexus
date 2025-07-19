using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Stat_Stunned : BooleanPrioritySolver, IStatTag { }
public class Stat_Channeling : BooleanPrioritySolver, IStatTag { }
public class Stat_Actions : ListPrioritySolver<Action>, IStatTag { }
public class Stat_Initiative : NumericalSolver, IStatTag { }
public class Mechanic_Actions : Mechanic<Mechanic_Actions>
{
    [FoldoutGroup("Actions")]
    public int tick = -1;
    [FoldoutGroup("Actions")]
    public int actionsPerTurn;
    public float TimePerAction => GameManager.Instance.timePerTurn / actionsPerTurn;
    public int TicksPerAction => (int)(TimePerAction / Time.fixedDeltaTime);
    private int phase;
    [ShowInInspector]
    private Action currentAction;

    public override void Tick()
    {
        if (Owner.Stat<Stat_Stunned>().Value) return;
        tick++;
        FetchNextAction();
        DoCurrentAction();
    }

    /// <summary>
    /// Adds an action to the end of the action list, ignoring maximum phases
    /// </summary>
    public void AddAction(Action action)
    {
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
        if (Owner.Stat<Stat_Actions>().Value.Count != actionsPerTurn)
        {
            List<Action> newList = new List<Action>();
            List<Action> newOverrideList = new List<Action>();
            for (int i = 0; i < actionsPerTurn; i++)
            {
                if (Owner.Stat<Stat_Actions>().Value.Count > i)
                {
                    newList.Add(Owner.Stat<Stat_Actions>().Value[i]);
                }
                else
                {
                    newList.Add(null);
                }
            }
            Owner.Stat<Stat_Actions>().Value = newList;
        }
    }

    private void FetchNextAction()
    {
        List<Action> actions = Owner.Stat<Stat_Actions>().Value as List<Action>;
        if (actions.Count == 0 || tick < Owner.Stat<Stat_Initiative>().Value) return;
        if (tick % TicksPerAction == 0)
        {
            currentAction?.OnEnd(Owner);
            if (Owner.Stat<Stat_Channeling>().Value)
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

                currentAction = actions[phase - 1];

                if(phase == 1)
                {
                    new Trigger_TurnStart(Owner, Owner);
                }
            }
            currentAction?.OnStart(Owner);
            new Trigger_ActionStart(currentAction, Owner, currentAction);
        }
    }

    private void DoCurrentAction()
    {
        currentAction?.Tick(Owner, TicksPerAction, tick);
    }
}