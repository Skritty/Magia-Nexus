using System;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public enum NumericalModifierCalculationMethod
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
    public NumericalModifierCalculationMethod Method { get; set; }
    public NumericalSolver() { }
    public NumericalSolver(float value, NumericalModifierCalculationMethod method)
    { 
        Value = value;
        Method = method;
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public static NumericalSolver CreateCalculation()
    {
        return new NumericalSolver(1, true);
    }

    protected NumericalSolver(float baseValue, bool root = false)
    {
        Value = baseValue;

        if (!root) return;
        Method = NumericalModifierCalculationMethod.Multiplicative;
        // Flat
        Modifiers.Add(new NumericalModifier(NumericalModifierCalculationMethod.Flat, 0));
        // Increased
        Modifiers.Add(new NumericalModifier(NumericalModifierCalculationMethod.Additive, 1));
        // More
        /*if (source != null) TODO: add source
        {
            modifiers.Add(new NumericalModifier(source.effectMultiplier, source));
        }*/
    }

    public void AddModifier(IModifier<float> modifier, NumericalModifierCalculationMethod method)
    {
        switch (method)
        {
            case NumericalModifierCalculationMethod.Flat:
                {
                    Modifiers[0].Modifiers.Add(modifier);
                    break;
                }
            case NumericalModifierCalculationMethod.Additive:
                {
                    Modifiers[1].Modifiers.Add(modifier);
                    break;
                }
            case NumericalModifierCalculationMethod.Multiplicative:
                {
                    Modifiers.Add(modifier);
                    break;
                }
        }
    }

    public override void Solve()
    {
        foreach (IModifier<float> modifier in Modifiers)
        {
            modifier.Solve();
            // If a non-NumericalModifier leaf with a positive value set its absolute
            /*if(modifier.absolute == 0 && modifier.value > 0)
            {
                modifier.absolute = modifier.value;
            }*/

            switch (Method)
            {
                case NumericalModifierCalculationMethod.Flat:
                case NumericalModifierCalculationMethod.Additive:
                    {
                        Value += modifier.Value * modifier.Stacks;
                        //absolute += modifier.absolute;
                        break;
                    }
                case NumericalModifierCalculationMethod.Multiplicative:
                    {
                        Value *= Mathf.Pow(modifier.Value, modifier.Stacks);
                        //absolute *= modifier.absolute;
                        break;
                    }
            }
        }
    }

    /*public override void InverseSolve()
    {
        foreach (Modifier<float> modifier in modifiers)
        {
            modifier.Value = Value / modifier.Value;
            modifier.InverseSolve();

            // (1) * (1 -0.5) * 1 = (0.5, 1) / (2.5, 3)
            // (0.16, 0.33), (0.083, 0.33), (0.16, 0.33)
            //         1=(0.16, 0.33), -0.5=(-0.083, 0)
        }
        if (!source.Owner || !source.Target) return;
        source.Owner.GetMechanic<Stat_PlayerOwner>().ApplyContribution(source.Target, Math.Abs(Value));
    }*/
}
