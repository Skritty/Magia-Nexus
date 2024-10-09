using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_PlayerOwner : GenericStat<Stat_PlayerOwner>
{
    [FoldoutGroup("Player Owned")]
    public bool playerCharacter;
    [FoldoutGroup("Player Owned")]
    public TMPro.TextMeshProUGUI characterNamePlate;
    [FoldoutGroup("Player Owned")]
    public Entity playerEntity;
    [FoldoutGroup("Player Owned")]
    public Viewer player;
    public Dictionary<Viewer, float> assists = new Dictionary<Viewer, float>();

    public static float pointsPerKill = 10;

    public void SetPlayer(Viewer player, Entity playerCharacter)
    {
        this.player = player;
        playerEntity = playerCharacter;
        if (playerCharacter)
        {
            Owner.name = player.viewerName;
            if (characterNamePlate != null)
                characterNamePlate.text = player.viewerName;
        }
    }

    public void SetPlayer(Stat_PlayerOwner inherit)
    {
        this.player = inherit.player;
        playerEntity = inherit.playerEntity;
    }

    public void Proxy(System.Action<Entity> method, bool doToSelf = true)
    {
        if(doToSelf)
            method?.Invoke(Owner);
        if (Owner != playerEntity)
            method?.Invoke(playerEntity);
    }

    public void ApplyContribution(Entity target, float effectMultiplier)
    {
        if (playerEntity == target) return;
        target = target.Stat<Stat_PlayerOwner>().playerEntity;
        Viewer player = playerEntity.Stat<Stat_PlayerOwner>().player;
        if (!target.Stat<Stat_PlayerOwner>().assists.TryAdd(player, Mathf.Abs(effectMultiplier)))
        {
            target.Stat<Stat_PlayerOwner>().assists[player] += Mathf.Abs(effectMultiplier);
        }
        //Debug.Log($"{playerEntity.name} gaining {effectMultiplier} contribution points towards the kill on {target.name} | Now at {target.Stat<Stat_PlayerOwner>().assists[player]}");
    }

    public void DistributeRewards()
    {
        float totalContribution = 0;
        foreach(KeyValuePair<Viewer, float> assist in assists)
        {
            totalContribution += assist.Value;
        }

        foreach (KeyValuePair<Viewer, float> assist in assists)
        {
            float points = pointsPerKill * (assist.Value / totalContribution);
            assist.Key.points += points;
            assist.Key.currency += (int)points;
        }
    }
}