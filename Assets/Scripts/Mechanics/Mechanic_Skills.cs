using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_ThinkingTime : NumericalSolver, IStat<float> { }
public class Stat_Skills : ListStat<Action>, IStat<Action> { }
public class Stat_SkillConditions : ListStat<SkillCondition>, IStat<SkillCondition> { }
public class Stat_Stunned : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Channeling : BooleanPrioritySolver, IStat<bool> { }
public class Stat_Initiative : NumericalSolver, IStat<float> { }
public class Trigger_SkillTriggerCheck : Trigger<Trigger_SkillTriggerCheck, Entity> { }
public class Trigger_DefaultSkill : Trigger<Trigger_DefaultSkill, Entity> { }
public class Mechanic_Skills : Mechanic<Mechanic_Skills>
{
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private SkillCondition queuedSkill;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private Action activeSkill;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private int tick;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private bool thinking = true;
    private System.Action cleanup;
    public Dictionary<SkillCondition, Action> conditionBindings = new();

    public override void Initialize()
    {
        for(int i = 0; i < Owner.Stat<Stat_Skills>().Count; i++)
        {
            BindAction(Owner.Stat<Stat_Skills>()[i], Owner.Stat<Stat_SkillConditions>()[i]);
        }
        ConditionSetup();
    }

    public void ConditionSetup()
    {
        cleanup?.Invoke();
        cleanup = null;
        foreach (SkillCondition condition in Owner.Stat<Stat_SkillConditions>())
        {
            cleanup += condition.condition.SubscribeToTasks(Owner, 0);
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
        if(queuedSkill != null) Debug.Log($"{queuedSkill.name} priority = {Owner.Stat<Stat_SkillConditions>().IndexOf(queuedSkill)} | " +
            $"{condition.name} priority = {Owner.Stat<Stat_SkillConditions>().IndexOf(condition)}");
        if (queuedSkill != null && Owner.Stat<Stat_SkillConditions>().IndexOf(queuedSkill) < Owner.Stat<Stat_SkillConditions>().IndexOf(condition)) return;
        queuedSkill = condition;
    }

    public override void Tick()
    {
        if (Owner.Stat<Stat_Stunned>().Value)
        {
            EndSkill();
            return;
        }

        tick++;
        
        if (thinking)
        {
            if (tick >= Owner.Stat<Stat_ThinkingTime>().Value)
            {
                thinking = false;
                Trigger_SkillTriggerCheck.Invoke(Owner, Owner);
                if (queuedSkill == null)
                {
                    Trigger_DefaultSkill.Invoke(Owner, Owner);
                }
            }
            return;
        }

        if (activeSkill == null && queuedSkill != null)
        {
            activeSkill = conditionBindings[queuedSkill];
            queuedSkill = null;
            tick = 0;
        }

        if(activeSkill && activeSkill.Tick(Owner, 25, tick)) // TODO: remove ticks per action
        {
            EndSkill();
        }
    }

    public void EndSkill()
    {
        activeSkill = null;
        tick = 0;
        thinking = true;
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
        owner.GetMechanic<Mechanic_Skills>().QueueSkill(condition);
        return true;
    }
}
