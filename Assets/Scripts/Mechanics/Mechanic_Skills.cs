using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_ThinkingTime : NumericalSolver, IStat<float> { }
public class Trigger_SkillTriggerCheck : Trigger<Trigger_SkillTriggerCheck, Entity> { }
public class Trigger_DefaultSkill : Trigger<Trigger_DefaultSkill, Entity> { }
public class Mechanic_Skills : Mechanic<Mechanic_Skills>
{
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private Skill queuedSkill;
    [FoldoutGroup("Skills"), ReadOnly, ShowInInspector]
    private Skill activeSkill;
    [FoldoutGroup("Skills")]
    public List<Skill> skills;
    private int tick;
    private bool thinking = true;

    public override void Initialize()
    {
        foreach (Skill skill in skills)
        {
            skill.skillTriggerCondition.SubscribeToTasks(Owner, null);
            skill.skillTriggerCondition.triggered += x => QueueSkill(x, skill);
        }
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

        if (activeSkill == null && queuedSkill)
        {
            activeSkill = queuedSkill;
            queuedSkill = null;
            tick = 0;
            activeSkill.Initialize();
        }
        if(activeSkill.Tick(Owner, tick))
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

    public void QueueSkill(object owner, Skill skill)
    {
        if (!Owner.Equals(owner) || (queuedSkill != null && queuedSkill.priority >= skill.priority)) return;
        queuedSkill = skill;
    }
}
