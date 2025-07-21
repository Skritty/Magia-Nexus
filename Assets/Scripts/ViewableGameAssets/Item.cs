using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/Item")]
public class Item : ViewableGameAsset
{
    public int cost;
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
            effect.DoTask(null, entity);
        }
    }
}