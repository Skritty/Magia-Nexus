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
    public List<EffectTask<Effect>> itemEffects = new();

    public void OnGained(Entity entity)
    {
        foreach(EffectTask<Effect> effect in itemEffects)
        {
            if(effect == null)
            {
                Debug.LogWarning($"{name} has a null effect!");
                continue;
            }
            effect.DoTask(null, entity);
        }
    }
}