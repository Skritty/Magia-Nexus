using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float timePerTurn;
    public int ticksPerTurn => (int)(timePerTurn * 50);
    public int defaultActionsPerTurn;
    public Phase initialPhase;
    [ShowInInspector, ReadOnly]
    private Phase currentPhase;
    private Coroutine phaseTimer;
    public TextMeshProUGUI timer;
    [ShowInInspector]
    public List<ViewableGameAsset> viewableGameAssets = new();
    [ShowInInspector]
    public List<ViewableGameAsset> allGameAssets = new();

    public Entity defaultPlayer;
    public List<string> defaultUnlockedClasses = new List<string>();
    [SerializeReference]
    public Personality defaultPersonality;
    public SerializedDictionary<string, List<Item>> startingClasses;
    public SerializedDictionary<string, Viewer> viewers = new SerializedDictionary<string, Viewer>();
    public SerializedDictionary<string, Viewer> allViewers = new SerializedDictionary<string, Viewer>();
    public static Viewer[] Viewers => Instance.viewers.Select(x => x.Value).ToArray();
    public static Viewer[] ViewersScoreOrdered => Instance.viewers.Select(x => x.Value).OrderBy(x => x.points).ToArray();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("join", Command_JoinGame);
        TwitchClient.Instance.AddCommand("leave", Command_LeaveGame);

        TwitchClient.Instance.AddCommand("gold", Command_CurrentGold);
        TwitchClient.Instance.AddCommand("points", Command_CurrentPoints);

        TwitchClient.Instance.AddCommand("items", Command_ListHeldItems);
        TwitchClient.Instance.AddCommand("actions", Command_ListActions);
        TwitchClient.Instance.AddCommand("turn", Command_ListTurn);
        TwitchClient.Instance.AddCommand("info", Command_Info);
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
        TwitchClient.Instance.RemoveCommand("info", Command_Info);
    }

    private CommandResponse Command_JoinGame(string user, List<string> args)
    {
        Viewer viewer;
        if (allViewers.ContainsKey(user))
        {
            // Existing player (this game)
            viewer = allViewers[user];
            viewer.gold = viewer.totalGold;
            viewer.items.Clear();
            viewers.Remove(user);
        }
        else
        {
            // New player (this game)
            viewer = new Viewer();
            viewer.viewerName = user;
            viewer.personality = defaultPersonality;
            foreach (string c in defaultUnlockedClasses) viewer.unlockedClasses.Add(c);
            allViewers.Add(user, viewer);
        }
        
        // Pick class
        if (args.Count == 0 || !startingClasses.ContainsKey(args[0]))
        {
            string classes = "";
            foreach(string c in viewer.unlockedClasses)
            {
                if (startingClasses[c].Count == 0) continue;
                classes += $"{c}, ";
            }
            classes = classes.Remove(classes.Length -2, 2);
            return new CommandResponse(false, $"Please pick a valid class out of: {classes}");
        }
        else if(!viewer.unlockedClasses.Contains(args[0]))
        {
            return new CommandResponse(false, $"You do not have {args[0]} unlocked");
        }
        
        viewer.items.AddRange(startingClasses[args[0]]);
        viewers.Add(viewer.viewerName, viewer);

        TwitchClient.Instance.SendChatMessage($"@{user} joined as {args[0]}");
        return new CommandResponse(true, "");
    }

    private CommandResponse Command_LeaveGame(string user, List<string> args)
    {
        if (viewers.ContainsKey(user))
        {
            allViewers.Add(user, viewers[user]);
            viewers.Remove(user);
        }
        
        TwitchClient.Instance.SendChatMessage($"@{user} left the game");
        return new CommandResponse(true, "");
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

    public CommandResponse Command_ListHeldItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");

        string message = $"@{user} You have: ";
        Dictionary<string, int> itemAmounts = new();
        foreach (Item item in GameManager.Instance.viewers[user].items)
        {
            if (item.hidden) continue;
            if (itemAmounts.ContainsKey(item.name))
            {
                itemAmounts[item.name]++;
            }
            else
            {
                itemAmounts.Add(item.name, 1);
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

        return new CommandResponse(true, "");
    }

    public CommandResponse Command_ListTurn(string user, List<string> args)
    {
        if(args.Count > 0) return new CommandResponse(true, "");
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        string message = $"@{user} Your turn will be: ";
        foreach (Action action in GameManager.Instance.viewers[user].actions)
        {
            message += $"{action.name}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandResponse(true, "");
    }

    public CommandResponse Command_CurrentGold(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have {GameManager.Instance.viewers[user].gold} gold";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandResponse(true, "");
    }

    public CommandResponse Command_CurrentPoints(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have {GameManager.Instance.viewers[user].points} points. You gained {GameManager.Instance.viewers[user].roundPoints} last round.";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandResponse(true, "");
    }

    public CommandResponse Command_ListActions(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You have: ";
        HashSet<Action> actions = new HashSet<Action>();
        foreach (Item item in GameManager.Instance.viewers[user].items)
        {
            foreach(Action action in item.grantedActions)
            {
                if (actions.Contains(action)) continue;
                actions.Add(action);
                message += $"{action.name}, ";
            }
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandResponse(true, "");
    }

    public CommandResponse Command_Info(string user, List<string> args)
    {
        if (args.Count == 0) return new CommandResponse(true, "");
        string message = $"@{user} ";
        HashSet<Action> actions = new HashSet<Action>();

        string name = string.Join(" ", args);
        bool found = false;
        foreach (ViewableGameAsset asset in allGameAssets)
        {
            if (!asset.NameMatch(name)) continue;
            message += asset.name + ": " + asset.info;
            found = true;
            break;
        }
        if (!found)
        {
            return new CommandResponse(true, "");
        }

        TwitchClient.Instance.SendChatMessage(message);
        return new CommandResponse(true, "");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        viewableGameAssets.Clear();
        allGameAssets.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:ViewableGameAsset", new string[] { "Assets/Data/ViewableGameAssets" }))
        {
            ViewableGameAsset asset = (ViewableGameAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(ViewableGameAsset));
            if (asset == null) continue;
            if(!asset.hidden) viewableGameAssets.Add(asset);
            allGameAssets.Add(asset);
        }
    }
#endif
}