using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using System;
using Skritty.Tools.Utilities;

public class GameManager : Singleton<GameManager>
{
    public float timePerTurn;
    public int ticksPerTurn => (int)(timePerTurn * 50);
    public int defaultActionsPerTurn;
    public Phase initialPhase;
    private Phase currentPhase;
    private Coroutine phaseTimer;
    public TextMeshProUGUI timer;

    public Entity defaultPlayer;
    [SerializeReference]
    public Targeting defaultTargeting;
    public SerializedDictionary<string, List<Item>> startingClasses;
    public SerializedDictionary<string, Viewer> viewers = new SerializedDictionary<string, Viewer>();
    public SerializedDictionary<string, Viewer> inactiveViewers = new SerializedDictionary<string, Viewer>();
    public static Viewer[] Viewers => Instance.viewers.Select(x => x.Value).ToArray();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("join", Command_JoinGame);
        TwitchClient.Instance.AddCommand("leave", Command_LeaveGame);

        TwitchClient.Instance.AddCommand("gold", Command_CurrentGold);
        TwitchClient.Instance.AddCommand("points", Command_CurrentPoints);

        TwitchClient.Instance.AddCommand("items", Command_ListHeldItems);
        TwitchClient.Instance.AddCommand("actions", Command_ListActions);
        TwitchClient.Instance.AddCommand("turn", Command_ListTurn);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("join", Command_JoinGame);
        TwitchClient.Instance.RemoveCommand("leave", Command_LeaveGame);

        TwitchClient.Instance.RemoveCommand("gold", Command_CurrentGold);
        TwitchClient.Instance.RemoveCommand("points", Command_CurrentPoints);

        TwitchClient.Instance.RemoveCommand("items", Command_ListHeldItems);
        TwitchClient.Instance.RemoveCommand("actions", Command_ListActions);
        TwitchClient.Instance.RemoveCommand("turn", Command_ListTurn);
    }

    private CommandError Command_JoinGame(string user, List<string> args)
    {
        if (args.Count == 0 || !startingClasses.ContainsKey(args[0]))
        {
            string classes = "";
            foreach(KeyValuePair<string, List<Item>> c in startingClasses)
            {
                if (c.Value.Count == 0) continue;
                classes += $"{c.Key}, ";
            }
            classes = classes.Remove(classes.Length -2, 2);
            return new CommandError(false, $"Please pick a valid starting class out of: {classes}");
        }

        if (viewers.ContainsKey(user))
        {
            inactiveViewers.Add(user, viewers[user]);
            viewers.Remove(user);
        }

        Viewer viewer;
        if (inactiveViewers.ContainsKey(user))
        {
            viewer = inactiveViewers[user];
            viewer.gold = viewer.totalGold;
            viewer.items.Clear();
            inactiveViewers.Remove(user);
        }
        else viewer = new Viewer();

        viewer.viewerName = user;
        viewer.items.AddRange(startingClasses[args[0]]);
        viewer.targetType = defaultTargeting;
        viewers.Add(user, viewer);

        TwitchClient.Instance.SendChatMessage($"@{user} joined as {args[0]}");
        return new CommandError(true, "");
    }

    private CommandError Command_LeaveGame(string user, List<string> args)
    {
        if (viewers.ContainsKey(user))
        {
            inactiveViewers.Add(user, viewers[user]);
            viewers.Remove(user);
        }
        
        TwitchClient.Instance.SendChatMessage($"@{user} left the game");
        return new CommandError(true, "");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += SceneDoneLoading;
    }

    public void StartPhase(Phase phase)
    {
        EndPhase();
        currentPhase = phase;
        SceneManager.LoadScene(phase.sceneToLoad);
    }

    public void NextPhase()
    {
        StartPhase(currentPhase.nextPhase);
    }

    private void SceneDoneLoading(Scene scene, LoadSceneMode sceneMode)
    {
        currentPhase.OnEnter();
        if(phaseTimer != null) StopCoroutine(phaseTimer);
        phaseTimer = StartCoroutine(PhaseTimer(currentPhase));
    }

    private void EndPhase()
    {
        if (currentPhase == null) return;
        currentPhase.OnExit();
        currentPhase = null;
    }

    private IEnumerator PhaseTimer(Phase phase)
    {
        for (int i = 0; i < phase.tickDuration; i++)
        {
            timer.text = new DateTime().AddSeconds((int)((phase.tickDuration - i) / 50)).ToString("mm:ss");
            yield return new WaitForFixedUpdate();
        }
        StartPhase(phase.nextPhase);
    }

    public CommandError Command_ListHeldItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");

        string message = $"@{user} You have: ";
        Dictionary<string, int> itemAmounts = new();
        foreach (Item item in GameManager.Instance.viewers[user].items)
        {
            if (item.hidden) continue;
            if (itemAmounts.ContainsKey(item.ItemName))
            {
                itemAmounts[item.ItemName]++;
            }
            else
            {
                itemAmounts.Add(item.ItemName, 1);
            }
        }
        foreach (KeyValuePair<string, int> item in itemAmounts)
        {
            if (item.Value == 1) message += $"{item.Key}, ";
            else
            {
                message += $"{item.Key} ({item.Value}), ";
            }
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);

        return new CommandError(true, "");
    }

    public CommandError Command_ListTurn(string user, List<string> args)
    {
        if(args.Count > 0) return new CommandError(true, "");
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} Your turn will be: ";
        foreach (Action action in GameManager.Instance.viewers[user].actions)
        {
            message += $"{action.ActionName}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CurrentGold(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have {GameManager.Instance.viewers[user].gold} gold";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CurrentPoints(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have {GameManager.Instance.viewers[user].points} points. You gained {GameManager.Instance.viewers[user].roundPoints} last round.";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_ListActions(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have: ";
        HashSet<Action> actions = new HashSet<Action>();
        foreach (Item item in GameManager.Instance.viewers[user].items)
        {
            foreach(Action action in item.grantedActions)
            {
                if (actions.Contains(action)) continue;
                actions.Add(action);
                message += $"{action.ActionName}, ";
            }
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }
}