using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementManager : Singleton<AchievementManager>
{
    public List<Achievement> achievements; //TODO: Read from folder
    private Dictionary<Viewer, List<string>> completedAchievements = new Dictionary<Viewer, List<string>>();

    private void Start()
    {
        foreach(Viewer player in GameManager.Viewers)
        {
            LoadPlayerAchievements(player);
        }
        foreach(Achievement achievement in achievements)
        {
            achievement.Load();
        }
    }

    private void OnDestroy()
    {
        foreach (Achievement achievement in achievements)
        {
            achievement.Unload();
        }
    }

    public void LoadPlayerAchievements(Viewer player)
    {
        // read from database or something and look for player name
        //completedAchievements
        //achievement.GrantReward(player);
    }

    public void Save()
    {
        // Save to database or something
    }

    public void GrantAchievement(Achievement achievement, Viewer player)
    {
        completedAchievements.TryAdd(player, new List<string>());
        if (completedAchievements[player].Contains(achievement.name)) return;
        completedAchievements[player].Add(achievement.name);
        achievement.GrantReward(player);
        Debug.Log($"{player.viewerName} has earned {achievement.name}!");
    }
}
