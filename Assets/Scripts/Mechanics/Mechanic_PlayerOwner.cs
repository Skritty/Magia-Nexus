using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stat_PlayerCharacter : Stat<Entity>, IStatTag<Entity> { } // TODO: make this the end all be all for referencing the player character
public class Mechanic_PlayerOwner : Mechanic<Mechanic_PlayerOwner>
{
    [FoldoutGroup("Player Owned")]
    public Entity proxyOwner;
    [FoldoutGroup("Player Owned")]
    public bool scaleWithPlayerCharacterModifiers;
    [FoldoutGroup("Player Owned")]
    public TMPro.TextMeshProUGUI characterNamePlate;
    [FoldoutGroup("Player Owned")]
    public Entity playerEntity;
    [FoldoutGroup("Player Owned")]
    public Viewer player;
    [FoldoutGroup("Player Owned"), ShowInInspector, ReadOnly]
    public Dictionary<Viewer, float> assists = new Dictionary<Viewer, float>();

    public void SetPlayer(Viewer player, Entity playerCharacter)
    {
        this.player = player;
        player.character = Owner;
        playerEntity = playerCharacter;
        if (playerCharacter)
        {
            Owner.name = player.viewerName;
            if (characterNamePlate != null)
                characterNamePlate.text = player.viewerName;
        }
    }

    public void SetPlayer(Mechanic_PlayerOwner inherit)
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
        target = target.GetMechanic<Mechanic_PlayerOwner>().playerEntity;
        Viewer player = playerEntity.GetMechanic<Mechanic_PlayerOwner>().player;
        if (!target.GetMechanic<Mechanic_PlayerOwner>().assists.TryAdd(player, Mathf.Abs(effectMultiplier)))
        {
            target.GetMechanic<Mechanic_PlayerOwner>().assists[player] += Mathf.Abs(effectMultiplier);
        }
        //Debug.Log($"{playerEntity.name} gaining {effectMultiplier} contribution points towards the kill on {target.name} | Now at {target.Stat<Stat_PlayerOwner>().assists[player]}");
    }

    public void DistributeRewards()
    {
        player.killedBy.Clear();
        player.killedBy.AddRange(assists);
        float totalContribution = 0;
        foreach(KeyValuePair<Viewer, float> assist in assists)
        {
            totalContribution += assist.Value;
        }

        foreach (KeyValuePair<Viewer, float> assist in assists)
        {
            float points = assist.Key.killGainMultiplier * assist.Value / totalContribution;
            if (assist.Value / totalContribution == 0 || assist.Value / totalContribution == float.NaN) Debug.Log(totalContribution);
            assist.Key.points += points;
            assist.Key.roundPoints += points;
        }
    }
}