using TwitchLib.Api.Helix.Models.Search;
using UnityEngine;

public class Modifier_Scaling : Modifier<float>, ISolver<float>
{
    public override float Value 
    { 
        get => base.Value * (baseMultiplierReferenceValue / statReference.Value); 
        set => base.Value = value; 
    }

    public IDataContainer<float> statReference;
    public float baseMultiplierReferenceValue;

    public void Solve()
    {

    }

    public void InverseSolve()
    {

    }

    public void MarkAsChanged()
    {

    }
}
