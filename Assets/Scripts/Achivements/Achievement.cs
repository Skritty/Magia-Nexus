using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(menuName = "Achievement")]
public class Achievement : ViewableGameAsset
{
    public string description;
    [SerializeReference]
    public Trigger activationTrigger;
    [SerializeReference]
    public ITaskOwned<Viewer, dynamic> rewardTask;

    public System.Action Load(Viewer player)
    {
        return activationTrigger.SubscribeMethodToTasks(player.character, x => CompleteAchivement(player), 0);
    }

    public void CompleteAchivement(Viewer player)
    {
        AchievementManager.Instance.GrantAchievement(this, player);
    }

    public void GrantReward(Viewer player)
    {
        rewardTask?.DoTask(player, null);
    }
}
