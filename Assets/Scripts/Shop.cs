using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix;

public class Shop : MonoBehaviour
{
    public List<Item> shopItems = new List<Item>();
    public List<Action> basicTargeting = new List<Action>();
    
    public DisplayItem itemDisplayPrefab, actionDisplayPrefab, shopItemDisplayPrefab, basicTargetingDisplayPrefab;
    public GameObject placeholder;
    public RectTransform shopLayout, basicTargetingLayout, itemDisplayLayout;

    public List<Item> craftableItems = new List<Item>();

    private List<Item> ownedItems = new List<Item>();
    private List<Action> ownedActions = new List<Action>();
    private List<Action> ownedTargeting = new List<Action>();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("items", Command_ListHeldItems);
        TwitchClient.Instance.AddCommand("buy", Command_BuyItems);
        TwitchClient.Instance.AddCommand("craft", Command_CraftItem);

        TwitchClient.Instance.AddCommand("actions", Command_ListActions);
        TwitchClient.Instance.AddCommand("createturn", Command_CreateTurn);
        TwitchClient.Instance.AddCommand("targeting", Command_SetTargeting);
        TwitchClient.Instance.AddCommand("turn", Command_ListTurn);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("items");
        TwitchClient.Instance.RemoveCommand("buy");
        TwitchClient.Instance.RemoveCommand("craft");

        TwitchClient.Instance.RemoveCommand("actions");
        TwitchClient.Instance.RemoveCommand("createturn");
        TwitchClient.Instance.RemoveCommand("targeting");
        TwitchClient.Instance.RemoveCommand("turn");
    }

    private void Start()
    {
        ownedTargeting.AddRange(basicTargeting);
        FindAllAvailableActions();
        FindAllAvailableItems();
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        // Shop Items
        foreach(Item item in shopItems)
        {
            DisplayItem display = Instantiate(shopItemDisplayPrefab, shopLayout);
            display.title.text = item.itemName;
            display.desc.text = item.itemDescription;
            display.id.text = "i" + (ownedItems.IndexOf(item) + 1);
            display.cost.text = item.cost+"g";
        }

        // Basic Targeting
        foreach (Action action in basicTargeting)
        {
            DisplayItem display = Instantiate(basicTargetingDisplayPrefab, basicTargetingLayout);
            display.title.text = action.actionName;
            display.desc.text = action.actionDescription;
            display.id.text = "t" + (ownedTargeting.IndexOf(action) + 1);
        }

        // Items/Item Actions
        foreach (Item item in ownedItems)
        {
            if (shopItems.Contains(item)) continue;
            CheckForPlaceholder(itemDisplayLayout.childCount);

            DisplayItem display = Instantiate(itemDisplayPrefab, itemDisplayLayout);
            display.title.text = item.itemName;
            display.desc.text = item.itemDescription;
            display.id.text = "i" + (ownedItems.IndexOf(item) + 1);
            display.background.color = item.damageTypeColor;
            display.subboxes = item.grantedActions.Count;
            string recipe = "";
            foreach(Item ingredient in item.craftingRecipe)
            {
                recipe += "i" + (ownedItems.IndexOf(ingredient) + 1) + ", ";
            }
            //recipe = recipe.Remove(recipe.Length - 2, 2);
            display.recipe.text = recipe;

            int a = 0;
            foreach(Action action in item.grantedActions)
            {
                DisplayItem displayAction = Instantiate(actionDisplayPrefab, display.transform);
                displayAction.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(0, ++a * -displayAction.GetComponent<RectTransform>().sizeDelta.y);
                displayAction.title.text = action.actionName;
                displayAction.desc.text = action.actionDescription;
                switch (action.type)
                {
                    case ActionType.Targeting:
                        displayAction.id.text = "t" + (ownedTargeting.IndexOf(action) + 1); 
                        break;
                    case ActionType.Basic:
                    default:
                        displayAction.id.text = "a" + (ownedActions.IndexOf(action) + 1); 
                        break;
                }
                displayAction.background.color = item.damageTypeColor;
            }
        }
    }

    public void CheckForPlaceholder(int currentChildIndex)
    {
        // Check 3 positions back (or farther) if it takes multiple positions
        for (int i = 1; i < 10; i++)
        {
            if (currentChildIndex < 3 * i) break;
            DisplayItem above = itemDisplayLayout.GetChild(currentChildIndex - 3).GetComponent<DisplayItem>();
            if (above == null) continue;
            if (above.subboxes >= i)
            {
                Instantiate(placeholder, itemDisplayLayout);
                CheckForPlaceholder(1 + currentChildIndex);
                break;
            }
        }
    }

    public bool BuyItem(Viewer viewer, string toPurchase, out string message)
    {
        // Parse toPurcase + figure out what item to buy
        Item item = GetItemFromNameOrID(toPurchase);
        Debug.Log(viewer.viewerName + " bought " + item);
        if (!shopItems.Contains(item))
        {
            message = "Item is not purchaseable!";
            return false;
        }
        // Buy item if enough gold
        if(viewer.currency < item.cost)
        {
            message = "You don't have enough gold to purchase that!";
            return false;
        }
        viewer.currency -= item.cost;
        viewer.items.Add(item);
        message = $"{item.itemName}, ";
        return true;
    }

    public bool CraftItem(Viewer viewer, List<string> itemsUsedInCraft, out string message)
    {
        // Ensure that the viewer has the items
        List<Item> items = new List<Item>();
        foreach (string input in itemsUsedInCraft)
        {
            Item item = GetItemFromNameOrID(input);
            if (item == null || !viewer.items.Contains(item))
            {
                message = "You don't have one or more of the items used in the craft!";
                return false;
            }
            items.Add(item);
        }
        // Try crafting the item, and do stuff if it succeeds TODO: make this way better please
        HashSet<Item> possibleCrafts = new HashSet<Item>();
        foreach (Item item in craftableItems)
        {
            possibleCrafts.Add(item);
        }
        foreach (Item usedToCraft in items)
        {
            foreach (Item item in craftableItems)
            {
                if (item.craftingRecipe.Count != items.Count || !item.craftingRecipe.Contains(usedToCraft))
                {
                    possibleCrafts.Remove(item);
                }
            }
        }
        foreach (Item craft in possibleCrafts)
        {
            bool success = true;
            foreach (Item i in craft.craftingRecipe)
            {
                if (!items.Contains(i)) success = false;
            }
            if (success)
            {
                foreach(Item item in items)
                {
                    viewer.items.Remove(item);
                }
                viewer.items.Add(craft);
                message = craft.itemName;
                // TODO: Add new item to UI and owned
                return true;
            }
        }
        message = "No craft found with these items!";
        return false;
    }

    private Item GetItemFromNameOrID(string input)
    {
        int id;
        if (int.TryParse(input.Remove(0, 1), out id))
        {
            id--;
            if (id < 0 || id >= ownedItems.Count) return null;
            return ownedItems[id];
        }
        else
        {
            return ownedItems.Find(item => input.ToLower().Replace(" ", "").Equals(item.itemName.ToLower().Replace(" ", "")));
        }
    }

    private Action GetActionFromNameOrID(string input, List<Action> list)
    {
        int id;
        if (int.TryParse(input.Remove(0, 1), out id))
        {
            id--;
            if (id < 0 || id >= list.Count) return null;
            return list[id];
        }
        else
        {
            return list.Find((action) => input.ToLower().Replace(" ", "").Equals(action.actionName.ToLower().Replace(" ", "")));
        }
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
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            foreach (Item item in viewer.Value.items)
            {
                foreach (Action action in item.grantedActions)
                {
                    switch (action.type)
                    {
                        case ActionType.Targeting:
                            if (!ownedTargeting.Contains(action))
                            {
                                ownedTargeting.Add(action);
                            }
                            break;
                        case ActionType.Basic:
                        default:
                            if (!ownedActions.Contains(action))
                            {
                                ownedActions.Add(action);
                            }
                            break;
                    }
                    
                }
            }
        }
    }

    public void FindAllAvailableItems()
    {
        List<Item> items = new List<Item>();
        items.AddRange(shopItems);
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            foreach(Item item in viewer.Value.items)
            {
                if (!item.hidden && !items.Contains(item))
                {
                    items.Add(item);
                }
            }
        }
        ownedItems = items;
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
            Action action = GetActionFromNameOrID(input, this.ownedActions);
            if (action == null)
            {
                message = "This action doesn't exist!";
                return false;
            }
            if (!ownedActions.Contains(action))
            {
                message = $"None of your items grant you the action {action.actionName}! \nUse '!actions' to see what actions you can use!";
                return false;
            }
            actions.Add(action);
            turn += $" {action.actionName}, ";
        }
        viewer.actions = actions;
        message = $"Your new turn is:{turn.Remove(turn.Length - 2, 2)}!";
        return true;
    }

    public bool SetTargeting(Viewer viewer, string targetingType, out string message)
    {
        Action targetingAction = GetActionFromNameOrID(targetingType, ownedTargeting);
        if(targetingAction == null)
        {
            message = $"Targeting type does not exist!";
            return false;
        }
        else
        {
            message = targetingAction.actionName;
            //viewer.targetType = targetingAction; TODO
            return true;
        }
    }

    public void NextPhase() => GameManager.Instance.NextPhase();

    public CommandError Command_CheckGold(string user, List<string> args)
    {
        string message = $"@{user} You have: {GameManager.Instance.viewers[user].currency}";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_ListHeldItems(string user, List<string> args)
    {
        string message = $"@{user} You have: ";
        foreach (Item item in GameManager.Instance.viewers[user].items)
        {
            if (item.hidden) continue;
            message += $"{item.itemName}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_BuyItems(string user, List<string> args)
    {
        string message = $"@{user} You purchased: ";
        string outMessage;
        if(args.Count == 0)
        {
            return new CommandError(false, "Please enter items to buy...");
        }
        foreach(string itemName in args)
        {
            if(BuyItem(GameManager.Instance.viewers[user], itemName, out outMessage))
            {
                message += outMessage;
            }
            else
            {
                return new CommandError(false, outMessage);
            }
        }
        message = message.Remove(message.Length - 2, 2);
        message += $"! You now have {GameManager.Instance.viewers[user].currency}g";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CraftItem(string user, List<string> args)
    {
        string message = $"@{user} You crafted: ";
        string outMessage;
        if (CraftItem(GameManager.Instance.viewers[user], args, out outMessage))
        {
            message += outMessage;
        }
        else
        {
            return new CommandError(false, outMessage);
        }
        message += '!';
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_ListActions(string user, List<string> args)
    {
        string message = $"@{user} You have: ";
        foreach (Action action in FindAvailableActions(GameManager.Instance.viewers[user]))
        {
            message += $"{action.actionName}, ";
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
            message += $"{action.actionName}, ";
        }
        message = message.Remove(message.Length - 2, 2);
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_SetTargeting(string user, List<string> args)
    {
        if (args.Count == 0)
        {
            return new CommandError(false, $"{@GameManager.Instance.viewers[user].viewerName} Please provide a targeting type.");
        }

        string message = $"@{user} Your targeting behavior is now: ";
        string outMessage;

        if(SetTargeting(GameManager.Instance.viewers[user], args[0], out outMessage))
        {
            message += outMessage;
        }
        else
        {
            return new CommandError(false, outMessage);
        }
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }
}

// SHOP CHAT COMMANDS
// !items (shows items you possess)
// !buy 2, 2, 3, 1
// !craft 3, 1, 2