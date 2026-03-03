using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Recipe")]
public class FactoryRecipeBook : ScriptableObject
{
    [SerializeField]
    List<FactoryRecipe> recipies = new();

    public FactoryRecipe GetValidRecipe(Dictionary<FactoryMaterialType, FactoryMaterial> materials)
    {
        foreach (FactoryRecipe recipe in recipies)
        {
            if (recipe.Valid(materials)) return recipe;
        }
        return null;
    }
}

[Serializable]
public class FactoryRecipe
{
    public float productionTime;
    public FactoryMaterial outputMaterial;
    public List<FactoryMaterial> recipe = new();

    public bool Valid(Dictionary<FactoryMaterialType, FactoryMaterial> materials)
    {
        bool valid = true;
        foreach (FactoryMaterial material in recipe)
        {
            if (materials.ContainsKey(material.type) && materials[material.type].Amount >= material.Amount) continue;
            valid = false;
        }
        return valid;
    } 
}