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

    public void SetPlayer(Viewer player, Entity playerCharacter)
    {
        this.player = player;
        playerEntity = playerCharacter;
        if (playerCharacter)
        {
            owner.name = player.viewerName;
            if (characterNamePlate != null)
                characterNamePlate.text = player.viewerName;
        }
    }

    public void SetPlayer(Stat_PlayerOwner inherit)
    {
        this.player = inherit.player;
        playerEntity = inherit.playerEntity;
    }
}