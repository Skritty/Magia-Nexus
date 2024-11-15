using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrepPhase : MonoBehaviour
{
    public List<Action> availableActions = new List<Action>();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("actions", Command_ListActions);
        TwitchClient.Instance.AddCommand("createturn", Command_CreateTurn);
        TwitchClient.Instance.AddCommand("turn", Command_ListTurn);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("actions", Command_ListActions);
        TwitchClient.Instance.RemoveCommand("createturn", Command_CreateTurn);
        TwitchClient.Instance.RemoveCommand("turn", Command_ListTurn);
    }

    private void Start()
    {
        FindAllAvailableActions();
        UpdateActionDisplay();
    }

    public HashSet<Action> FindAvailableActions(Viewer viewer)
    {
        HashSet<Action> actions = new HashSet<Action>();
        foreach (Item item in viewer.items)
        {
            foreach (Action action in item.grantedActions)
            {
                if (!actions.Contains(action))
                {
                    actions.Add(action);
                }
            }
        }
        return actions;
    }

    public void FindAllAvailableActions()
    {
        HashSet<Action> actions = new HashSet<Action>();
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            actions.AddRange(FindAvailableActions(viewer.Value));
        }
        availableActions = actions.ToList();
    }

    public bool CreateTurn(Viewer viewer, List<string> turnActions, out string message)
    {
        // Check that the action list does not exceed max actions
        int turnActionCount = GameManager.Instance.defaultActionsPerTurn;
        foreach (Item item in viewer.items)
        {
            turnActionCount += item.actionCountModifier;
        }
        if (turnActionCount != turnActions.Count)
        {
            message = $"Your turn must contain exactly {turnActionCount} actions!";
            return false;
        }

        // Ensure that the viewer has the actions
        List<Action> actions = new List<Action>();
        HashSet<Action> ownedActions = FindAvailableActions(viewer);
        string turn = "";
        foreach (string input in turnActions)
        {
            Action action = GetActionFromNameOrID(input);
            if (action == null)
            {
                message = "This action doesn't exist!";
                return false;
            }
            if (!ownedActions.Contains(action))
            {
                message = $"None of your items grant you the action {action.ActionName}! \nUse '!actions' to see what actions you can use!";
                return false;
            }
            actions.Add(action);
            turn += $" {action.ActionName}, ";
        }
        viewer.actions = actions;
        message = $"Your new turn is:{turn.Remove(turn.Length - 2, 2)}!";
        return true;
    }

    private Action GetActionFromNameOrID(string input)
    {
        int id;
        if (int.TryParse(input, out id))
        {
            if (id < 0 || id > availableActions.Count) return null;
            return availableActions[id];
        }
        else
        {
            return availableActions.Find((action) => action.NameMatch(input));
        }
    }

    public void UpdateActionDisplay()
    {

    }

    public CommandError Command_ListActions(string user, List<string> args)
    {
        string message = $"@{user} You have: ";
        foreach (Action action in FindAvailableActions(GameManager.Instance.viewers[user]))
        {
            message += $"{action.ActionName}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CreateTurn(string user, List<string> args)
    {
        string message = "";
        string outMessage;
        if (CreateTurn(GameManager.Instance.viewers[user], args, out outMessage))
        {
            message += outMessage;
        }
        else
        {
            return new CommandError(false, outMessage);
        }
        TwitchClient.Instance.SendChatMessage($"@{user} " + message);
        return new CommandError(true, "");
    }

    public CommandError Command_ListTurn(string user, List<string> args)
    {
        string message = $"@{user} Your turn will be: ";
        foreach (Action action in GameManager.Instance.viewers[user].actions)
        {
            message += $"{action.ActionName}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }
}

// PREP PHASE CHAT COMMANDS
// !actions (show your available actions)
// !createTurn 2, 3, 5, 1, 2
// !turn (show your current turn)