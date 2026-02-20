using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;

[System.Serializable]
public abstract class NoisePass
{
    public abstract float NoiseMod(float noise, MultidimensionalPosition position);
}

public class NoisePass_Static : NoisePass
{
    public float toAdd;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        return noise + toAdd;
    }
}

public class NoisePass_PositionAxisAmount : NoisePass
{
    public int axis;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        return noise + position[axis];
    }
}

public class NoisePass_GradientNoise2D : NoisePass
{
    public float scale;
    public Vector2 noiseRange;
    public int axis1 = 0, axis2 = 2;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        Random.InitState(GetHashCode());
        Vector2 randomOffset = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));
        return noise + Mathf.Lerp(noiseRange.x, noiseRange.y, Noise.CalcPixel2D((int)(position[axis1] + randomOffset.x), (int)(position[axis2] + randomOffset.y), scale) / 255f);
    }
}

public class NoisePass_GradientNoise3D : NoisePass
{
    public float scale;
    public Vector2 noiseRange;
    public int axis1 = 0, axis2 = 1, axis3 = 2;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        Random.InitState(GetHashCode());
        Vector3 randomOffset = new Vector3(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));
        return noise + Mathf.Lerp(noiseRange.x, noiseRange.y, Noise.CalcPixel3D((int)(position[axis1] + randomOffset.x), (int)(position[axis2] + randomOffset.x), (int)(position[axis3] + randomOffset.x), scale) / 255f);
    }
}

public class NoisePass_Clamp : NoisePass
{
    public Vector2 clampRange;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        return Mathf.Clamp(noise, clampRange.x, clampRange.y);
    }
}

public class NoisePass_Scale : NoisePass
{
    public float center, scale;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        return (noise - center) / scale + center;
    }
}

public class NoisePass_AxialNoiseClamp : NoisePass
{
    public int axis;
    public bool invert;
    [SerializeReference]
    public List<NoisePass> clampedNoiseMax;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        float noiseMax = 0;
        foreach (NoisePass pass in clampedNoiseMax)
        {
            noiseMax = pass.NoiseMod(noise, position);
        }

        if (position[axis] > noiseMax) return invert ? noise : 0;
        else return invert ? 0 : noise;
    }
}

public class NoisePass_Step : NoisePass
{
    public float step;

    public override float NoiseMod(float noise, MultidimensionalPosition position)
    {
        return noise - noise % step;
    }
}