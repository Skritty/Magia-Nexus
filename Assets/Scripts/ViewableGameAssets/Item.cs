using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/Item")]
public class Item : ViewableGameAsset
{
    public ItemClassification classification;
    [SerializeField]
    private int cost;
    public int Cost
    {
        get
        {
            int c = cost;
            foreach (Item item in craftingRecipe)
            {
                c += item.Cost;
            }
            return c;
        }
    }
    public List<Item> craftingRecipe = new List<Item>();
    public List<Action> grantedActions = new List<Action>();
    public List<Personality> grantedPersonalities = new List<Personality>();
    public int actionCountModifier;
    [SerializeReference]
    public List<EffectTask> itemEffects = new();

    public void OnGained(Entity entity)
    {
        if (entity == null) return;
        foreach(EffectTask effect in itemEffects)
        {
            if(effect == null)
            {
                Debug.LogWarning($"{name} has a null effect!");
                continue;
            }
            effect.DoTaskNoData(entity);
        }
    }
}

[Flags]
public enum ItemClassification
{
    Immaterial = 0,
    Head = 1,
    Body = 2,
    Arms = 4,
    Belt = 8,
    Legs = 16,
    Feet = 32,
    OneHanded = 64,
    TwoHanded = 128,
    Back = 256,
    Neck = 512,
    Finger = 1024,
    Wrist = 2048,
    Trinket = 4096
}