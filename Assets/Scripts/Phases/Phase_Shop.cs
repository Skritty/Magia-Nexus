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
        TwitchClient.Instance.AddCommand("join", Command_GiveNewPlayerGold);
        roundsPast++;
        base.OnEnter();
        GivePlayersGold();
    }

    public override void OnExit()
    {
        TwitchClient.Instance.RemoveCommand("join", Command_GiveNewPlayerGold);
        base.OnExit();
    }

    private void GivePlayersGold()
    {
        foreach (Viewer viewer in GameManager.Viewers)
        {
            viewer.currency += baseGoldGain + (int)viewer.roundPoints;
            viewer.roundPoints = 0;
        }
    }

    private CommandError Command_GiveNewPlayerGold(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(true, "");

        Viewer viewer = GameManager.Instance.viewers[user];
        viewer.currency += baseGoldGain * roundsPast;

        return new CommandError(true, "");
    }
}