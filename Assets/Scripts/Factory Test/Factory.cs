using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Factory : MonoBehaviour
{
    public FactoryRecipeBook recipes;
    public int materialSlots;
    [ShowInInspector]
    public Dictionary<FactoryMaterialType, FactoryMaterial> materials = new();
    public List<Factory> outputs = new();
    private bool producing;

    private void Start()
    {
        RecipeCheck();
    }

    public void RecieveMaterial(FactoryMaterial material)
    {
        if (materials.ContainsKey(material.type))
        {
            materials[material.type].Amount += material.Amount;
        }
        else if(materials.Count < materialSlots)
        {
            materials.Add(material.type, new FactoryMaterial(material));
        }
        RecipeCheck();
    }

    public void RecipeCheck()
    {
        if (producing || recipes == null) return;
        FactoryRecipe recipe = recipes.GetValidRecipe(materials);
        if (recipe != null) StartCoroutine(Produce(recipe));
    }

    public IEnumerator Produce(FactoryRecipe factoryRecipe)
    {
        producing = true;
        yield return new WaitForSeconds(factoryRecipe.productionTime);
        foreach (FactoryMaterial material in factoryRecipe.recipe)
        {
            materials[material.type].Amount -= material.Amount;
        }
        foreach(Factory output in outputs)
        {
            output.RecieveMaterial(factoryRecipe.outputMaterial);
        }
        producing = false;
        RecipeCheck();
    }

    private void OnDrawGizmos()
    {
        foreach (Factory output in outputs)
        {
            Gizmos.DrawLine(transform.position, output.transform.position);
        }
    }
}

public enum FactoryMaterialType { OriginiumOre, FerriumOre, Sandleaf, SandleafSeed, Origiocrust, Ferrium, OriginiumPowder, FerriumPowder, SandleafPowder, DenseOriginiumPowder, DenseFerriumPowder, Steel, SteelPart, HCValleyBattery }

[Serializable]
public class FactoryMaterial
{
    public FactoryMaterialType type;
    public int max;
    [SerializeField]
    private int _amount;
    public int Amount
    {
        get { return _amount; }
        set
        {
            _amount = value;
            //_amount = Mathf.Clamp(_amount, 0, max);
        }
    }

    public FactoryMaterial() { }
    public FactoryMaterial(FactoryMaterial other)
    {
        type = other.type;
        max = other.max;
        _amount = other._amount;
    }
}