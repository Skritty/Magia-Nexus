using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill")]
public class Skill : ScriptableObject
{
    [SerializeReference]
    public Trigger skillTriggerCondition;
    public bool canOverrideQueuedSkill;
    public List<Action> actions = new();
    private Action currentAction;

    public void Initialize()
    {
        currentAction = actions[0];
    }

    public bool Tick(Entity owner, int tick)
    {
        if(currentAction.Tick(owner, currentAction.ActionTickDuration, tick))
        {
            int totalTickDuration = 0;
            for(int i = 0; i <= actions.Count; i++)
            {
                if(totalTickDuration >= tick)
                {
                    if(i >= actions.Count)
                    {
                        return true;
                    }
                    currentAction = actions[i];
                    break;
                }
                totalTickDuration += actions[i].ActionTickDuration;
            }
        }
        return false;
    }
}
