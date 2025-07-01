using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Achievement")]
public class Achievement : ViewableGameAsset
{
    public string description;
    [SerializeReference]
    public TriggerTask rewardTask;
    [SerializeReference]
    public List<Trigger> activationTriggers;
    [SerializeReference, HideReferenceObjectPicker]
    public List<TriggerTask> tasks = new List<TriggerTask>();
    private System.Action unsubscribe;

    public void Load()
    {
        foreach(Trigger trigger in activationTriggers)
        {
            unsubscribe += trigger.Subscribe(Check, null); // TODO
        }
    }

    public void Unload()
    {
        unsubscribe?.Invoke();
    }

    protected void Check(Trigger trigger)
    {
        if (trigger.Is(out IDataContainer_Player data))
        {
            bool allComplete = true;
            foreach (TriggerTask task in tasks)
            {
                if (!task.DoTask(trigger, null))
                {
                    allComplete = false;
                    break;
                }
            }
            if (allComplete)
            {
                AchievementManager.Instance.GrantAchievement(this, data.Player);
            }
        }
        else
        {
            //Debug.LogWarning($"{trigger.GetType()} is not valid with achievement {name}");
        }
    }

    public void GrantReward(Viewer player)
    {
        rewardTask?.DoTask(null, null);
    }
}
