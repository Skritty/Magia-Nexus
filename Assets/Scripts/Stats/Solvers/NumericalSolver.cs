using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CalculationStep
{
    Flat,
    Additive,
    Multiplicative
}

/// <summary>
/// A type of modifier that uses floats. 
/// It has built in functionality for flat, increased, and more nested calculation methods.
/// (flat + flat + ...) * (1 + increased + increased + ...) * more * more * ...
/// </summary>
[Serializable]
public class NumericalSolver : Stat<float>
{
    [FoldoutGroup("@GetType()")]
    public CalculationStep step;

    public NumericalSolver() { }

    public NumericalSolver(float value, CalculationStep method)
    { 
        _value = value;
        step = method;
    }

    public override void Solve()
    {
        _value = 0;
        foreach (IDataContainer<float> modifier in Modifiers)
        {
            if(modifier is Stat<float>)
            {
                (modifier as Stat<float>).Solve();
            }

            switch (step)
            {
                case CalculationStep.Flat:
                case CalculationStep.Additive:
                    {
                        _value += modifier.Value;
                        break;
                    }
                case CalculationStep.Multiplicative:
                    {
                        _value *= modifier.Value;
                        break;
                    }
            }
        }
    }
}