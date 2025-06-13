using System;
using System.Diagnostics;

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
public class NumericalModifier : Modifier<float>
{
    public NumericalModifierCalculationMethod method;
    public NumericalModifier() { }
    public NumericalModifier(float value, NumericalModifierCalculationMethod method)
    { 
        this.value = value;
        this.method = method;
    }

    /// <summary>
    /// Creates a modifier calculation that accepts flat, increased, and more modifiers
    /// </summary>
    public static NumericalModifier CreateCalculation(Effect source = null)
    {
        return new NumericalModifier(1, source, true);
    }

    protected NumericalModifier(float baseValue, Effect source = null, bool root = false)
    {
        this.source = source;
        this.value = baseValue;

        if (!root) return;
        method = NumericalModifierCalculationMethod.Multiplicative;
        // Flat
        submodifiers.Add(new NumericalModifier(NumericalModifierCalculationMethod.Flat, 0, source));
        // Increased
        submodifiers.Add(new NumericalModifier(NumericalModifierCalculationMethod.Additive, 1, source));
        // More
        if (source != null)
        {
            submodifiers.Add(new NumericalModifier(source.effectMultiplier, source));
        }
    }

    protected NumericalModifier(NumericalModifierCalculationMethod calculationType, float baseValue, Effect source = null)
    {
        this.source = source;
        this.method = calculationType;
        if(baseValue != 0)
            submodifiers.Add(new NumericalModifier(baseValue, source));
    }

    public void AddModifier(Modifier<float> modifier, NumericalModifierCalculationMethod method)
    {
        switch (method)
        {
            case NumericalModifierCalculationMethod.Flat:
                {
                    submodifiers[0].submodifiers.Add(modifier);
                    break;
                }
            case NumericalModifierCalculationMethod.Additive:
                {
                    submodifiers[1].submodifiers.Add(modifier);
                    break;
                }
            case NumericalModifierCalculationMethod.Multiplicative:
                {
                    submodifiers.Add(modifier);
                    break;
                }
        }
    }

    public override float Solve()
    {
        foreach (Modifier<float> modifier in submodifiers)
        {
            modifier.Solve();
            // If a non-NumericalModifier leaf with a positive value set its absolute
            /*if(modifier.absolute == 0 && modifier.value > 0)
            {
                modifier.absolute = modifier.value;
            }*/

            switch (method)
            {
                case NumericalModifierCalculationMethod.Flat:
                case NumericalModifierCalculationMethod.Additive:
                    {
                        value += modifier.value;
                        //absolute += modifier.absolute;
                        break;
                    }
                case NumericalModifierCalculationMethod.Multiplicative:
                    {
                        value *= modifier.value;
                        //absolute *= modifier.absolute;
                        break;
                    }
            }
        }
        return value;
    }

    public override void InverseSolve()
    {
        foreach (Modifier<float> modifier in submodifiers)
        {
            modifier.value = value / modifier.value;
            modifier.InverseSolve();

            // (1) * (1 -0.5) * 1 = (0.5, 1) / (2.5, 3)
            // (0.16, 0.33), (0.083, 0.33), (0.16, 0.33)
            //         1=(0.16, 0.33), -0.5=(-0.083, 0)
        }
        if (!source.Owner || !source.Target) return;
        source.Owner.Stat<Stat_PlayerOwner>().ApplyContribution(source.Target, Math.Abs(value));
    }
}
