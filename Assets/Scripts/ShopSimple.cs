using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSimple : MonoBehaviour
{
    public TextMeshProUGUI players;
    public DisplayItem itemDisplayPrefab, personalityDisplayPrefab;
    public RectTransform itemDisplayLayout, personalityDisplayLayout;
    public float scrollSpeed;

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("buy", Command_BuyItems);
        TwitchClient.Instance.AddCommand("sell", Command_SellItems);

        TwitchClient.Instance.AddCommand("turn", Command_CreateTurn);
        TwitchClient.Instance.AddCommand("personality", Command_SetPersonality);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("buy", Command_BuyItems);
        TwitchClient.Instance.RemoveCommand("sell", Command_SellItems);

        TwitchClient.Instance.RemoveCommand("turn", Command_CreateTurn);
        TwitchClient.Instance.RemoveCommand("personality", Command_SetPersonality);
    }

    public void Start()
    {
        CreateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void CreateUI()
    {
        int i = 0;
        foreach (ViewableGameAsset asset in GameManager.Instance.viewableGameAssets)
        {
            i++;
            if (asset.hidden) continue;
            CreateItemUI(asset as Item, itemDisplayPrefab, itemDisplayLayout, i);
            CreatePersonalityUI(asset as Personality, personalityDisplayPrefab, personalityDisplayLayout, i);
        }
    }

    private void UpdateUI()
    {
        players.text = "";
        foreach (Viewer player in GameManager.Viewers)
        {
            players.text += player.viewerName + "\n";
        }

        if(itemDisplayLayout.childCount >= 18)
        {
            Vector3 offset = itemDisplayLayout.offsetMax;
            offset.y += scrollSpeed / 2 * Time.deltaTime;
            if(offset.y > 54)
            {
                offset.y = 0;
                itemDisplayLayout.GetChild(0).SetSiblingIndex(itemDisplayLayout.childCount - 1);
                itemDisplayLayout.GetChild(0).SetSiblingIndex(itemDisplayLayout.childCount - 1);
            }
            itemDisplayLayout.offsetMax = offset;
        }

        if (personalityDisplayLayout.childCount >= 36)
        {
            Vector3 offset = personalityDisplayLayout.offsetMax;
            offset.y += scrollSpeed * Time.deltaTime;
            if (offset.y > 54)
            {
                offset.y = 0;
                personalityDisplayLayout.GetChild(0).SetSiblingIndex(personalityDisplayLayout.childCount - 1);
            }
            personalityDisplayLayout.offsetMax = offset;
        }
    }

    public void NextPhase() => GameManager.Instance.NextPhase();

    private void CreateItemUI(Item item, DisplayItem displayPrefab, Transform parent, int id)
    {
        if (item == null) return;

        DisplayItem display = Instantiate(displayPrefab, parent);
        display.title.text = item.name;
        display.id.text = ""+id;
        display.cost.text = item.Cost+"g";
        display.background.color = item.UIColor;
    }
    private void CreatePersonalityUI(ViewableGameAsset asset, DisplayItem displayPrefab, Transform parent, int id)
    {
        if (asset == null) return;

        DisplayItem display = Instantiate(displayPrefab, parent);
        display.title.text = asset.name;
        display.id.text = "" + id;
        display.background.color = asset.UIColor;
    }

    private ViewableGameAsset GetViewableAssetFromNameOrID(string itemNameOrID)
    {
        /*int.TryParse(itemNameOrID, out int id);
        if (id > 0)
        {
            if (GameManager.Instance.viewableGameAssets.Count <= id) return null;
            ViewableGameAsset item = GameManager.Instance.viewableGameAssets[id];
            return item;
        }*/

        int i = -1;
        foreach (ViewableGameAsset asset in GameManager.Instance.viewableGameAssets)
        {
            i++;
            if (asset.NameMatch(itemNameOrID)) return asset;
        }

        return null;
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

    public CommandResponse Command_BuyItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");

        Viewer player = GameManager.Instance.viewers[user];
        List<Item> purchasedItems = new();
        List<Item> failedItems = new();
        List<string> invalidItems = new();
        
        if (args.Count == 0)
        {
            return new CommandResponse(false, "Please enter items to buy");
        }
        else
        {
            Item item = null;
            string itemName = "";
            foreach (string arg in args)
            {
                itemName += arg;
                item = GetViewableAssetFromNameOrID(itemName) as Item;

                if (item == null)
                {
                    if (int.TryParse(itemName, out _)) itemName = "";
                    invalidItems.Add(itemName);
                    continue;
                }
                else
                {
                    itemName = "";
                    if (player.gold < item.Cost)
                    {
                        failedItems.Add(item);
                        continue;
                    }

                    player.gold -= item.Cost;
                    player.items.Add(item);
                    purchasedItems.Add(item);
                }
            }
        }

        string message = "";
        if(purchasedItems.Count > 0)
        {
            message += "Purchased: " + string.Join(", ", purchasedItems) + " | ";
        }
        if (failedItems.Count > 0)
        {
            message += "Too expensive: " + string.Join(", ", failedItems) + " | ";
        }
        if (invalidItems.Count > 0)
        {
            message += "Invalid: " + string.Join(", ", invalidItems) + " | ";
        }
        message += $"You now have {GameManager.Instance.viewers[user].gold}g";
        return new CommandResponse(true, message);
    }

    public CommandResponse Command_SellItems(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");

        Viewer player = GameManager.Instance.viewers[user];
        List<Item> soldItems = new();
        bool turnInvalidated = false;

        if (args.Count == 0)
        {
            return new CommandResponse(false, "Please enter items to sell");
        }
        else
        {
            Item item = null;
            string itemName = "";
            foreach (string arg in args)
            {
                itemName += arg;
                item = GetViewableAssetFromNameOrID(itemName) as Item;

                if (item == null)
                {
                    if (int.TryParse(itemName, out _)) itemName = "";
                    continue;
                }
                else
                {
                    if (!player.items.Contains(item)) return new CommandResponse(false, $"You don't have {item.name}.");
                    itemName = "";
                    player.gold += item.Cost;
                    player.items.Remove(item);
                    soldItems.Add(item);

                    foreach(Action action in player.actions)
                    {
                        if (!FindAvailableActions(player).Contains(action))
                        {
                            player.actions.Clear();
                            turnInvalidated = true;
                            break;
                        }
                    }
                }
            }
        }

        string message = "";
        if (soldItems.Count > 0)
        {
            message += "Sold: " + string.Join(",", soldItems) + " | ";
        }
        if (turnInvalidated)
        {
            message += "WARNING! Your turn was cleared, please remake it! | ";
        }
        message += $"You now have {GameManager.Instance.viewers[user].gold}g";
        return new CommandResponse(true, message);
    }

    public CommandResponse Command_CreateTurn(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        if (args.Count == 0) return new CommandResponse(true, "");

        Viewer player = GameManager.Instance.viewers[user];
        HashSet<Action> ownedActions = FindAvailableActions(player);
        List<string> invalidActions = new();
        bool notAtMaxActions = false;
        bool fullyInvalid = true;

        // Check that the action list does not exceed max actions
        int turnActionCount = GameManager.Instance.defaultActionsPerTurn;
        foreach (Item item in player.items)
        {
            turnActionCount += item.actionCountModifier;
        }

        List<Action> actions = new();
        for (int i = 0; i < turnActionCount; i++)
        {
            if (args.Count <= i)
            {
                actions.Add(null);
                notAtMaxActions = true;
            }
            else
            {
                Action action = GetViewableAssetFromNameOrID(args[i]) as Action;
                if(action == null)
                {
                    actions.Add(null);
                }
                else
                {
                    if(!FindAvailableActions(player).Contains(action)) return new CommandResponse(false, $"You don't have {action.name}.");
                    actions.Add(action);
                    fullyInvalid = false;
                }
            }
        }

        if (fullyInvalid)
        {
            return new CommandResponse(false, "Invalid turn! Use '!actions' to see what actions you can use!");
        }

        player.actions = actions;

        string message = $"Your new turn is: " + string.Join(", ", actions);
        if (invalidActions.Count > 0)
        {
            message += " | Invalid: " + string.Join(", ", invalidActions);
        }
        if (notAtMaxActions)
        {
            message += " | WARNING: You are not at max actions!"; 
        }
        if (args.Count > turnActionCount)
        {
            message += " | WARNING: You input more actions than you can use in a turn!";
        }
        return new CommandResponse(true, message);
    }

    public CommandResponse Command_SetPersonality(string user, List<string> args)
    {
        if (!GameManager.Instance.viewers.ContainsKey(user)) return new CommandResponse(false, "Use \'!join\" to join the game!");
        if (args.Count == 0)
        {
            return new CommandResponse(false, "Please provide a personality!");
        }

        string name = string.Join(" ", args);
        Personality personality = (Personality)GetViewableAssetFromNameOrID(name);
        
        if(personality == null)
        {
            return new CommandResponse(false, "Invalid personality!");
        }
        else
        {
            GameManager.Instance.viewers[user].personality = personality;
            return new CommandResponse(true, $"Personality now set to: " +personality.name);
        }
    }
}
