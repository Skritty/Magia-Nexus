using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TwitchLib.Api.Helix.Models.Extensions.ReleasedExtensions;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(menuName = "Achievement")]
public class Achievement : ViewableGameAsset
{
    public string description;
    [SerializeReference]
    public Trigger activationTrigger;
    [SerializeReference]
    public TriggerTask rewardTask;

    public System.Action Load(Viewer player)
    {
        return activationTrigger.AddTaskOwner(player.character, new []{ new AchievementComplete(this) });
    }

    public class AchievementComplete : TriggerTask<Entity>
    {
        public AchievementComplete(Achievement achievement)
        {
            this.achievement = achievement;
        }

        public Achievement achievement;
        public override bool DoTask(Entity data, Entity Owner)
        {
            AchievementManager.Instance.GrantAchievement(achievement, Owner);
            return true;
        }
    }

    public void GrantReward(Viewer player)
    {
        rewardTask?.DoTaskNoData(null);
    }
}
