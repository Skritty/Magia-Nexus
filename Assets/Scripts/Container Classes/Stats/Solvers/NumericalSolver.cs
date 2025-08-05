using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CalculationStep
{
    Flat,
    Additive,
    Multiplicative
}

public interface ICalculationComponent
{
    public CalculationStep Step { get; }
}

/// <summary>
/// A type of modifier that uses floats. 
/// It has built in functionality for flat, increased, and more nested calculation methods.
/// (flat + flat + ...) * (1 + increased + increased + ...) * more * more * ...
/// </summary>
[Serializable]
public class NumericalSolver : Solver<float>, ICalculationComponent
{
    [field: SerializeField, FoldoutGroup("@GetType()"), HideIf("@this is IStat")]
    public CalculationStep Step { get; protected set; }

    public NumericalSolver() { }

    public NumericalSolver(CalculationStep method)
    {
        Step = method;
    }

    public NumericalSolver(float value, CalculationStep method)
    {
        Modifiers.Add(new DataContainer<float>(value));
        Step = method;
    }

    public override void Solve()
    {
        switch (Step)
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
            (modifier as Solver<float>)?.Solve();

            switch (Step)
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

    public NumericalSolver Clone()
    {
        NumericalSolver clone = (NumericalSolver)MemberwiseClone();
        clone.Step = Step;
        return clone;
    }
}