using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix;
using UnityEngine.Windows;
using System;
using TwitchLib.Api.Helix.Models.Teams;
using TMPro;

public class Shop : MonoBehaviour
{
    public List<Item> shopItems = new List<Item>();
    public List<TargetingShopItem> basicTargeting = new List<TargetingShopItem>();
    [Serializable]
    public class TargetingShopItem
    {
        [SerializeReference]
        public Targeting targeting;
        public string name;
        public string description;
    }
    
    public DisplayItem itemDisplayPrefab, actionDisplayPrefab, shopItemDisplayPrefab, basicTargetingDisplayPrefab;
    public GameObject placeholder;
    public RectTransform shopLayout, basicTargetingLayout, itemDisplayLayout;
    public TextMeshProUGUI playerList; // Temp

    public List<Item> craftableItems = new List<Item>();

    private List<Item> ownedItems = new List<Item>();
    private List<Action> ownedActions = new List<Action>();
    private List<TargetingShopItem> ownedTargeting = new List<TargetingShopItem>();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("join", Command_NewPlayerJoined);

        TwitchClient.Instance.AddCommand("buy", Command_BuyItems);
        TwitchClient.Instance.AddCommand("sell", Command_SellItems);
        TwitchClient.Instance.AddCommand("craft", Command_CraftItem);
        
        TwitchClient.Instance.AddCommand("turn", Command_CreateTurn);
        TwitchClient.Instance.AddCommand("target", Command_SetTargeting);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("join", Command_NewPlayerJoined);

        TwitchClient.Instance.RemoveCommand("buy", Command_BuyItems);
        TwitchClient.Instance.RemoveCommand("sell", Command_SellItems);
        TwitchClient.Instance.RemoveCommand("craft", Command_CraftItem);
        
        TwitchClient.Instance.RemoveCommand("turn", Command_CreateTurn);
        TwitchClient.Instance.RemoveCommand("target", Command_SetTargeting);
    }

    private void Start()
    {
        ownedTargeting.AddRange(basicTargeting);
        FindAllOwnedItems();
        foreach (Item item in craftableItems) // TODO: Placeholder (shows all items)
        {
            if (ownedItems.Contains(item)) continue;
            ownedItems.Add(item);
        }
        FindAllAvailableActions();
        UpdateDisplay();
        UpdatePlayerList();
    }

    public void UpdateDisplay()
    {
        // Shop Items
        foreach(Item item in shopItems)
        {
            DisplayItem display = Instantiate(shopItemDisplayPrefab, shopLayout);
            display.title.text = item.ItemName;
            display.desc.text = item.itemDescription;
            display.id.text = "i" + (ownedItems.IndexOf(item) + 1);
            display.cost.text = item.cost + "g";
        }

        // Basic Targeting
        foreach (TargetingShopItem targeting in basicTargeting)
        {
            DisplayItem display = Instantiate(basicTargetingDisplayPrefab, basicTargetingLayout);
            display.title.text = targeting.name;
            display.desc.text = targeting.description;
            display.id.text = "t" + (ownedTargeting.IndexOf(targeting) + 1);
        }

        // Items/Item Actions
        foreach (Item item in ownedItems)
        {
            CreateItemUI(item);
        }
    }

    public void UpdatePlayerList()
    {
        playerList.text = "";
        foreach (Viewer player in GameManager.Viewers)
        {
            playerList.text += $"\n{player.viewerName}";
        }
    }

    private void CreateItemUI(Item item)
    {
        if (shopItems.Contains(item)) return;
        CheckForPlaceholder(itemDisplayLayout.childCount);

        DisplayItem display = Instantiate(itemDisplayPrefab, itemDisplayLayout);
        display.title.text = item.ItemName;
        display.desc.text = item.itemDescription;
        display.id.text = "i" + (ownedItems.IndexOf(item) + 1);
        display.background.color = item.damageTypeColor;
        display.subboxes = item.grantedActions.Count;
        string recipe = "";
        foreach (Item ingredient in item.craftingRecipe)
        {
            recipe += "i" + (ownedItems.IndexOf(ingredient) + 1) + ", ";
        }
        if(recipe.Length > 0)
            recipe = recipe.Remove(recipe.Length - 2, 2);
        display.recipe.text = recipe;

        int a = 0;
        foreach (Action action in item.grantedActions)
        {
            DisplayItem displayAction = Instantiate(actionDisplayPrefab, display.transform);
            displayAction.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0, ++a * -displayAction.GetComponent<RectTransform>().sizeDelta.y);
            displayAction.title.text = action.ActionName;
            displayAction.desc.text = action.actionDescription;
            displayAction.id.text = "a" + (ownedActions.IndexOf(action) + 1);
            displayAction.background.color = item.damageTypeColor;
        }

        foreach (TargetingShopItem targeting in item.grantedTargeting)
        {
            DisplayItem displayAction = Instantiate(actionDisplayPrefab, display.transform);
            displayAction.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0, ++a * -displayAction.GetComponent<RectTransform>().sizeDelta.y);
            displayAction.title.text = targeting.name;
            displayAction.desc.text = targeting.description;
            displayAction.id.text = "t" + (ownedTargeting.IndexOf(targeting) + 1);
            displayAction.background.color = item.damageTypeColor;
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
        if(item == null)
        {
            message = $"{toPurchase} does not exist!";
            return false;
        }
        if (!shopItems.Contains(item) && item.craftingRecipe.Count == 0)
        {
            message = $"{toPurchase} is not purchaseable!";
            return false;
        }
        // Buy item if enough gold
        if(viewer.gold < item.cost)
        {
            message = $"You don't have enough gold to purchase {toPurchase}!";
            return false;
        }
        if (!shopItems.Contains(item))
        {
            // check if can craft
            int totalGoldCost = item.cost;
            List<Item> itemsHeld = new List<Item>();
            Queue<Item> components = new Queue<Item>();
            foreach (Item i in item.craftingRecipe)
            {
                components.Enqueue(i);
            }
            while (components.Count > 0)
            {
                Item component = components.Dequeue();
                if (viewer.items.Count(x => x == component) > itemsHeld.Count(x => x == component))
                {
                    itemsHeld.Add(component);
                    continue;
                }
                totalGoldCost += component.cost;
                foreach (Item i in component.craftingRecipe)
                {
                    components.Enqueue(i);
                }
            }

            if(totalGoldCost > viewer.gold)
            {
                message = $"You only have {viewer.gold}g out of {totalGoldCost}g to buy a {item.ItemName}";
                return false;
            }

            foreach(Item component in itemsHeld) viewer.items.Remove(component);
            viewer.gold -= totalGoldCost;
            viewer.items.Add(item);
            message = $"{item.ItemName}, ";
        }
        else
        {
            viewer.gold -= item.cost;
            viewer.items.Add(item);
            message = $"{item.ItemName}, ";
        }
        
        return true;
    }

    public bool SellItem(Viewer viewer, string toSell, ref int gold, ref bool turnInvalidated, out string message)
    {
        Item item = GetItemFromNameOrID(toSell);
        if (item == null)
        {
            message = $"No item with that name found!";
            return false;
        }
        if (!viewer.items.Contains(item))
        {
            message = $"Cannot sell an item you do not have!";
            return false;
        }
        if(item.craftingRecipe.Count == 0 && item.cost == 0)
        {
            message = $"{item.ItemName} is unsellable!";
            return false;
        }

        viewer.items.Remove(item);

        int totalGoldCost = 0;
        Queue<Item> components = new Queue<Item>();
        components.Enqueue(item);

        while(components.Count > 0)
        {
            Item component = components.Dequeue();
            totalGoldCost += component.cost;
            foreach (Item i in component.craftingRecipe)
            {
                components.Enqueue(i);
            }
        }

        viewer.gold += totalGoldCost;

        gold += totalGoldCost;
        message = $"{item.ItemName}, ";

        foreach (Action action in item.grantedActions)
        {
            if (viewer.items.Exists(x => x.grantedActions.Contains(action))) continue;
            if (!viewer.actions.Contains(action)) continue;
            viewer.actions.Clear();
            turnInvalidated = true;
            break;
        }

        return true;
    }

    public bool CraftItem(Viewer viewer, string itemToCraft, out string message)
    {
        Item craft = GetItemFromNameOrID(itemToCraft);
        if (craft == null)
        {
            message = "Item does not exist";
            return false;
        }
        if(craft.craftingRecipe.Count == 0)
        {
            message = "This item cannot be crafted";
            return false;
        }
        List<Item> components = new();
        components.AddRange(craft.craftingRecipe);
        foreach (Item item in viewer.items)
        {
            components.Remove(item);
        }
        if(components.Count > 0)
        {
            message = "You don't have ";
            foreach (Item component in components)
            {
                message += component.ItemName + ", ";
            }
            message = message.Remove(message.Length - 2, 2);
            return false;
        }
        foreach (Item material in craft.craftingRecipe)
        {
            viewer.items.Remove(material);
        }
        viewer.items.Add(craft);
        message = craft.ItemName;
        if (!ownedItems.Contains(craft))
        {
            ownedItems.Add(craft);
            foreach (Action action in craft.grantedActions)
            {
                if (!ownedActions.Contains(action))
                {
                    ownedActions.Add(action);
                }
            }
            foreach (TargetingShopItem targeting in craft.grantedTargeting)
            {
                if (!ownedTargeting.Contains(targeting))
                {
                    ownedTargeting.Add(targeting);
                }
            }
            CreateItemUI(craft);
        }
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
                message = craft.ItemName;
                if (!ownedItems.Contains(craft))
                {
                    ownedItems.Add(craft);
                    foreach (Action action in craft.grantedActions)
                    {
                        if (!ownedActions.Contains(action))
                        {
                            ownedActions.Add(action);
                        }
                    }
                    foreach (TargetingShopItem targeting in craft.grantedTargeting)
                    {
                        if (!ownedTargeting.Contains(targeting))
                        {
                            ownedTargeting.Add(targeting);
                        }
                    }
                    CreateItemUI(craft);
                }
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
            return ownedItems.Find(item => item.NameMatch(input));
        }
    }

    private Action GetActionFromNameOrID(string input)
    {
        int id;
        if (int.TryParse(input.Remove(0, 1), out id))
        {
            id--;
            if (id < 0 || id >= ownedActions.Count) return null;
            return ownedActions[id];
        }
        else
        {
            return ownedActions.Find((action) => action.NameMatch(input));
        }
    }

    private TargetingShopItem GetTargetingFromNameOrID(string input)
    {
        int id;
        if (int.TryParse(input.Remove(0, 1), out id))
        {
            id--;
            if (id < 0 || id >= ownedTargeting.Count) return null;
            return ownedTargeting[id];
        }
        else
        {
            return ownedTargeting.Find((targeting) => input.ToLower().Replace(" ", "").Equals(targeting.name.ToLower().Replace(" ", "")));
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
        foreach (Item item in ownedItems)
        {
            foreach (Action action in item.grantedActions)
            {
                if (!ownedActions.Contains(action))
                {
                    ownedActions.Add(action);
                }
            }
        }
    }

    public void FindAllAvailableTargeting()
    {
        foreach (Item item in ownedItems)
        {
            foreach (TargetingShopItem targeting in item.grantedTargeting)
            {
                if (!ownedTargeting.Contains(targeting))
                {
                    ownedTargeting.Add(targeting);
                }
            }
        }
    }

    public void FindAllOwnedItems()
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

    public bool SetTargeting(Viewer viewer, string targetingType, bool targetLock, out string message)
    {
        TargetingShopItem targeting = GetTargetingFromNameOrID(targetingType);
        if(targeting == null)
        {
            message = $"Targeting type does not exist!";
            return false;
        }
        else
        {
            message = $"{targeting.name} is {(targetLock ? "locked" : "unlocked")}";
            viewer.targetType = targeting.targeting.Clone();
            viewer.targetType.lockTarget = targetLock;
            return true;
        }
    }

    public void NextPhase() => GameManager.Instance.NextPhase();

    public CommandError Command_BuyItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You purchased: ";
        string outMessage;
        if(args.Count == 0)
        {
            return new CommandError(false, "Please enter items to buy");
        }
        if(args.Count == 2 && int.TryParse(args[0], out int amt) && amt > 0)
        {
            amt = Mathf.Clamp(amt, 0, 100);
            string m2 = "";
            for(int i = 1; i <= amt; i++)
            {
                if (BuyItem(GameManager.Instance.viewers[user], args[1], out outMessage))
                {
                    m2 = $"{i} "+outMessage;
                }
                else
                {
                    if(i == 1)
                    {
                        message = $"Failed to purchase {amt - (i - 1)} {args[0]}";
                        
                    }
                    else
                    {
                        message += m2;
                        message = message.Remove(message.Length - 2, 2);
                        message += $"! Failed to purchase {amt - (i - 1)}";
                    }
                    return new CommandError(false, message);
                }
            }
            message += m2;
        }
        else
        {
            List<string> failed = new List<string>();
            foreach (string itemName in args)
            {
                if (BuyItem(GameManager.Instance.viewers[user], itemName, out outMessage))
                {
                    message += outMessage;
                }
                else if(args.Count == 1)
                {
                    return new CommandError(false, outMessage);
                }
                else
                {
                    failed.Add(itemName);
                }
            }
            if(failed.Count > 0)
            {
                message = message.Remove(message.Length - 2, 2);
                message += $"! Failed to purchase ";
            }
            foreach (string itemName in failed)
            {
                message += $"{itemName}, ";
            }
        }
        
        message = message.Remove(message.Length - 2, 2);
        message += $"! You now have {GameManager.Instance.viewers[user].gold}g";
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_SellItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} Sold ";
        string outMessage;
        bool turnInvalidated = false;
        int gold = 0;

        if (args.Count == 0)
        {
            return new CommandError(false, "Please enter items to sell");
        }
        if (args.Count == 2 && int.TryParse(args[0], out int amt) && amt > 0)
        {
            amt = Mathf.Clamp(amt, 0, 100);
            string m2 = "";
            for (int i = 1; i <= amt; i++)
            {
                if (SellItem(GameManager.Instance.viewers[user], args[1], ref gold, ref turnInvalidated, out outMessage))
                {
                    m2 = $"{i} {outMessage}";
                }
                else if (args.Count == 1)
                {
                    return new CommandError(false, outMessage);
                }
            }
            message += m2;
        }
        else
        {
            foreach (string itemName in args)
            {
                if (SellItem(GameManager.Instance.viewers[user], itemName, ref gold, ref turnInvalidated, out outMessage))
                {
                    message += outMessage;
                }
                else if (args.Count == 1)
                {
                    return new CommandError(false, outMessage);
                }
            }
            
        }

        message = message.Remove(message.Length - 2, 2);
        message += $" for {gold}g! ";
        if (turnInvalidated)
        {
            message += $"Your turn is no longer valid! Please remake your turn!";
        }
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CraftItem(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        string message = $"@{user} You crafted: ";
        string outMessage;
        if (args.Count == 1)
        {
            if (CraftItem(GameManager.Instance.viewers[user], args[0], out outMessage))
            {
                message += outMessage;
            }
            else
            {
                return new CommandError(false, outMessage);
            }
        }
        else if(args.Count > 0)
        {
            if (CraftItem(GameManager.Instance.viewers[user], args, out outMessage))
            {
                message += outMessage;
            }
            else
            {
                return new CommandError(false, outMessage);
            }
        }
        message += '!';
        TwitchClient.Instance.SendChatMessage(message);
        return new CommandError(true, "");
    }

    public CommandError Command_CreateTurn(string user, List<string> args)
    {
        if (args.Count == 0) return new CommandError(true, "");
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
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

    public CommandError Command_SetTargeting(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(false, "Use \'!join\" to join the game!");
        if (args.Count == 0)
        {
            return new CommandError(false, $"{@GameManager.Instance.viewers[user].viewerName} Please provide a targeting type.");
        }

        string message = $"@{user} Your targeting behavior is now: ";
        string outMessage;

        bool targetLock = false;
        if (args.Count > 1 && args[1].ToLower() == "lock")
        {
            targetLock = true;
        }
        if(SetTargeting(GameManager.Instance.viewers[user], args[0], targetLock, out outMessage))
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

    public CommandError Command_NewPlayerJoined(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandError(true, "");

        UpdatePlayerList();
        Viewer viewer = GameManager.Instance.viewers[user];
        foreach (Item item in viewer.items)
        {
            if (ownedItems.Contains(item)) continue;
            ownedItems.Add(item);
            CreateItemUI(item);

            foreach (Action action in item.grantedActions)
            {
                if (ownedActions.Contains(action)) continue;
                ownedActions.Add(action);
            }

            foreach (TargetingShopItem targeting in item.grantedTargeting)
            {
                if (ownedTargeting.Contains(targeting)) continue;
                ownedTargeting.Add(targeting);
            }
        }

        return new CommandError(true, "");
    }
}

// SHOP CHAT COMMANDS
// !items (shows items you possess)
// !buy 2, 2, 3, 1
// !craft 3, 1, 2