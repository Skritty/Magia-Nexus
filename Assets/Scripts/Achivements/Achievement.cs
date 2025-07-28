using UnityEngine;

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
