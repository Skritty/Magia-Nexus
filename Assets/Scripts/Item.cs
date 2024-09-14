using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject
{
    public bool hidden;
    public string itemName;
    [TextArea]
    public string itemDescription;
    public int cost;
    public Sprite itemImage;
    public Color damageTypeColor = Color.white;
    public List<Item> craftingRecipe = new List<Item>();
    public List<Action> grantedActions = new List<Action>();
    public int actionCountModifier;
    [SerializeReference]
    public List<Effect> itemEffects = new();

    public void OnGained(Entity entity)
    {
        foreach(Effect effect in itemEffects)
        {
            effect.Create(this, entity);
        }
    }
}