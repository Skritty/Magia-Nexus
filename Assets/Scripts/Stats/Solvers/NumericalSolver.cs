using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
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
    [FoldoutGroup("@GetType()"), HideIf("@this is IStatTag")]
    public CalculationStep step;

    public NumericalSolver() { }

    public NumericalSolver(CalculationStep method)
    {
        step = method;
    }

    public NumericalSolver(float value, CalculationStep method)
    {
        Modifiers.Add(new DataContainer<float>(value));
        step = method;
    }

    public override void Solve()
    {
        switch (step)
        {
            case CalculationStep.Flat:
                {
                    _value = 0;
                    break;
                }
            case CalculationStep.Additive:
            case CalculationStep.Multiplicative:
                {
                    _value = 1;
                    break;
                }
        }
        
        foreach (IDataContainer<float> modifier in Modifiers)
        {
            (modifier as Stat<float>)?.Solve();

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

    public override Stat Clone()
    {
        NumericalSolver clone = (NumericalSolver)base.Clone();
        clone.step = step;
        return clone;
    }
}