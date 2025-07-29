using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/Item")]
public class Item : ViewableGameAsset
{
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