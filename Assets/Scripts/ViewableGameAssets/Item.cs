using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
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
    public List<Effect> itemEffects = new();

    public void OnGained(Entity entity)
    {
        foreach(Effect effect in itemEffects)
        {
            effect.Create(entity);
        }
    }
}