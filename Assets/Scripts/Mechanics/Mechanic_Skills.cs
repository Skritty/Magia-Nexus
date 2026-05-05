using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_MaximumFocus : NumericalSolver, IStat<float> { }
public class Stat_CurrentFocus : NumericalSolver, IStat<float> { }
public class Stat_FocusRecoveryRate : NumericalSolver, IStat<float> { }
public class Stat_FocusDepleationStunDuration : NumericalSolver, IStat<float> { }
public class Stat_Skills : ListStat<Action>, IStat<Action> { }
public class Stat_SkillConditions : ListStat<SkillCondition>, IStat<SkillCondition> { }
public class Stat_SkillReactions : ListStat<SkillCondition>, IStat<SkillCondition> { }
public class Stat_Stunned : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Channeling : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Initiative : NumericalSolver, IStat<float> { }
public class Trigger_SkillTriggerCheck : Trigger<Trigger_SkillTriggerCheck, Entity> { }
public class Trigger_DefaultSkill : Trigger<Trigger_DefaultSkill, Entity> { }
public class Mechanic_Skills : Mechanic
{
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private SkillCondition queuedTrigger;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private Action activeSkill;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private int tick;
    private System.Action cleanup;
    [FoldoutGroup("Skills")]
    public SerializedDictionary<SkillCondition, Action> conditionBindings = new();

    public override void Initialize()
    {
        ConditionSetup();
        Owner.GetStat<Stat_CurrentFocus>().Value = Owner.GetStat<Stat_MaximumFocus>().Value;
    }

    public void ConditionSetup()
    {
        cleanup?.Invoke();
        cleanup = null;
        if(conditionBindings.Count > 0)
        {
            foreach (var binding in conditionBindings)
            {
                cleanup += binding.Key.condition.SubscribeToTasks(Owner, 0);
            }
        }
        else
        {
            foreach (SkillCondition condition in Owner.GetStat<Stat_SkillConditions>())
            {
                cleanup += condition.condition.SubscribeToTasks(Owner, 0);
            }
            foreach (SkillCondition condition in Owner.GetStat<Stat_SkillReactions>())
            {
                cleanup += condition.condition.SubscribeToTasks(Owner, 0);
            }
        }
    }

    public void BindAction(Action action, SkillCondition condition)
    {
        if (conditionBindings.ContainsKey(condition))
        {
            conditionBindings.Remove(condition);
        }
        conditionBindings.Add(condition, action);
    }

    public void QueueSkill(SkillCondition condition)
    {
        if (!conditionBindings.ContainsKey(condition)) return;
        if(queuedTrigger != null) Debug.Log($"{queuedTrigger.name} priority = {Owner.GetStat<Stat_SkillConditions>().IndexOf(queuedTrigger)} | " +
            $"{condition.name} priority = {Owner.GetStat<Stat_SkillConditions>().IndexOf(condition)}");

        if (queuedTrigger != null && !Owner.GetStat<Stat_SkillConditions>().Contains(queuedTrigger) && Owner.GetStat<Stat_SkillConditions>().IndexOf(queuedTrigger) < Owner.GetStat<Stat_SkillConditions>().IndexOf(condition)) return;
        queuedTrigger = condition;
    }

    public override void Tick()
    {
        if (Owner.GetStat<Stat_CurrentFocus>().Value == 0)
        {
            Owner.GetStat<Stat_Stunned>().AddModifier(new Modifier_Priority<bool>(value: true, priority: byte.MaxValue, tickDuration: (int)Owner.GetStat<Stat_FocusDepleationStunDuration>().Value));
        }

        if (Owner.GetStat<Stat_Stunned>().Value)
        {
            EndSkill();
            return;
        }

        Owner.GetStat<Stat_CurrentFocus>().Value = Mathf.Clamp(Owner.GetStat<Stat_CurrentFocus>().Value + Owner.GetStat<Stat_FocusRecoveryRate>().Value / 50f, 0, Owner.GetStat<Stat_MaximumFocus>().Value);

        if (activeSkill == null)
        {
            if(queuedTrigger == null)
            {
                Trigger_SkillTriggerCheck.Invoke(Owner, Owner);
                if (queuedTrigger == null)
                {
                    Trigger_DefaultSkill.Invoke(Owner, Owner);
                }
                if (queuedTrigger == null) return;
            }

            TryStartSkill(conditionBindings[queuedTrigger]);
        }
        
        if (activeSkill)
        {
            if (queuedTrigger && activeSkill.Cancelable(tick, conditionBindings[queuedTrigger]))
            {
                TryStartSkill(conditionBindings[queuedTrigger]);
            }

            float cost = activeSkill.FocusCost(tick);
            if (!float.IsNaN(cost))
            {
                Owner.GetStat<Stat_CurrentFocus>().Value = Mathf.Clamp(Owner.GetStat<Stat_CurrentFocus>().Value - cost, 0, Owner.GetStat<Stat_MaximumFocus>().Value);
            }

            if (activeSkill.Tick(Owner, tick)) EndSkill();

            tick++;
        }
    }

    public bool TryStartSkill(Action skill)
    {
        if (conditionBindings[queuedTrigger].focusCost <= Owner.GetStat<Stat_CurrentFocus>().Value)
        {
            EndSkill();
            tick = 0;
            activeSkill = skill;
            queuedTrigger = null;
            activeSkill.OnStart(Owner);
            return true;
        }
        return false;
    }

    public void EndSkill()
    {
        activeSkill?.OnStart(Owner);
        activeSkill = null;
    }
}

public class Task_QueueSkill<T> : ITaskOwned<Entity, T>
{
    public SkillCondition condition;
    public bool DoTask(T data)
    {
        return false; 
    }

    public bool DoTask(Entity owner, T data)
    {
        // ID: based on the index in Stat_SkillConditions
        owner.GetStat<Mechanic_Skills>().QueueSkill(condition);
        return true;
    }
}
