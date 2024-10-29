using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Phases/Shop")]
public class Phase_Shop : Phase
{
    public static int roundsPast = 0;
    public int baseGoldGain;
    public override void OnEnter()
    {
        roundsPast++;
        base.OnEnter();
        GivePlayersGold();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    private void GivePlayersGold()
    {
        foreach (Viewer viewer in GameManager.Viewers)
        {
            viewer.currency += baseGoldGain * (viewer.newPlayer ? roundsPast : 1) + (int)viewer.roundPoints;
            viewer.newPlayer = false;
            viewer.roundPoints = 0;
        }
    }
}