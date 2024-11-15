using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject
{
    public bool hidden;
    public List<string> itemAliases = new List<string>();
    public string ItemName => itemAliases.Count == 0 ? "" : itemAliases[0];
    public bool NameMatch(string s)
    {
        s = s.ToLower().Replace(" ", "");
        foreach (string name in itemAliases)
        {
            if (name.ToLower().Replace(" ", "").Equals(s)) return true;
        }
        return false;
    }
    [TextArea]
    public string itemDescription;
    public int cost;
    public Sprite itemImage;
    public Color damageTypeColor = Color.white;
    public List<Item> craftingRecipe = new List<Item>();
    public List<Action> grantedActions = new List<Action>();
    public List<Targeting> grantedTargeting = new List<Targeting>();
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