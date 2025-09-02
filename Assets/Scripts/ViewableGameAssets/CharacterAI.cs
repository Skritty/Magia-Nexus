using System;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ViewableGameAsset/AI")]
public class CharacterAI : ViewableGameAsset
{
    public bool ignoreRequirements;
    public int maxItemsPurchased = 1;
    public List<WeightedAsset<Item>> itemPurchasingBehavior;
    public List<WeightedAsset<Action>> turnCreationBehavior;
    public List<WeightedAsset<Personality>> personalitySettingBehavior;

    public void PurchaseItems(Viewer viewer)
    {
        Item item = null;
        int amount = 0;
        do
        {
            List<WeightedAsset<Item>> weightedAssets = new();
            WeightedChance<WeightedAsset<Item>> random = new();
            foreach (WeightedAsset<Item> asset in itemPurchasingBehavior)
            {
                if (!ignoreRequirements && asset.asset != null && asset.asset.Cost > viewer.gold) continue;
                random.Add(asset, asset.weight);
                weightedAssets.Add(asset);
            }

            item = GetWeightedAsset(itemPurchasingBehavior, random, x =>
            {
                if(viewer != null)
                {
                    return viewer.items.Count(y => y == x);
                }
                return 0;
            });

            if (item == null) break;

            if (viewer != null)
            {
                viewer.gold -= item.Cost;
                viewer.items.Add(item);
            }
            //item.OnGained(entity);
            amount++;
        }
        while (amount < maxItemsPurchased);
    }

    public void CreateTurn(Viewer viewer)
    {
        HashSet<Action> availableActions = FindAvailableActions(viewer);
        if (!ignoreRequirements && availableActions.Count == 0) return;
        Action[] turnActions = new Action[GetActionCount(viewer)];

        List<WeightedAsset<Action>> weightedAssets = new();
        WeightedChance<WeightedAsset<Action>> random = new();
        foreach (WeightedAsset<Action> asset in turnCreationBehavior)
        {
            if (!ignoreRequirements && !availableActions.Contains(asset.asset)) continue;
            random.Add(asset, asset.weight);
            weightedAssets.Add(asset);
        }
        for(int i = 0; i < turnActions.Length; i++)
        {
            turnActions[i] = GetWeightedAsset(weightedAssets, random, x => turnActions.Count(y => y == x));
        
        }
        //entity?.AddModifier<List<Action>, Stat_Actions>(turnActions, 0);
        if (viewer != null)
        {
            viewer.actions.Clear();
            viewer.actions.AddRange(turnActions);
        }
    }

    public void SetPersonality(Viewer viewer)
    {
        // TODO: add unlockable personalities
        WeightedChance<WeightedAsset<Personality>> random = new();
        foreach (WeightedAsset<Personality> asset in personalitySettingBehavior)
        {
            random.Add(asset, asset.weight);
        }
        Personality personality = GetWeightedAsset(personalitySettingBehavior, random, x => 0);
        //if (entity != null) personality.SetPersonality(entity);
        if (viewer != null) viewer.personality = personality;
    }

    public HashSet<Action> FindAvailableActions(Viewer viewer)
    {
        HashSet<Action> actions = new HashSet<Action>();
        if (viewer == null) return actions;
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

    public int GetActionCount(Viewer viewer)
    {
        int count = 5;
        if (viewer == null) return count;
        foreach (Item item in viewer.items)
        {
            count += item.actionCountModifier;
        }
        return count;
    }

    public T GetWeightedAsset<T>(List<WeightedAsset<T>> list, WeightedChance<WeightedAsset<T>> random, Func<T, int> currentAmount) where T : ViewableGameAsset
    {
        int stop = 0;
        HashSet<WeightedAsset<T>> ignore = new();
        WeightedAsset<T> pick = null;
        while (pick == null && stop < 100)
        {
            pick = random.GetRandomEntry();
            float overrideChance = UnityEngine.Random.Range(0f, 1f);
            foreach (WeightedAsset<T> asset in list)
            {
                if (!ignore.Contains(asset) && overrideChance <= asset.percentageOverride)
                {
                    pick = asset;
                }
            }
            if (pick.max != 0 && currentAmount.Invoke(pick.asset) >= pick.max)
            {
                ignore.Add(pick);
                pick = null;
            }
            stop++;
        }
        return pick.asset;
    }
}

[Serializable]
public class WeightedAsset<T> where T : ViewableGameAsset
{
    public T asset;
    public float weight = 1;
    [Range(0f, 1f)]
    public float percentageOverride;
    public int max;
}
