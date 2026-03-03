using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;

[System.Serializable]
public abstract class NoisePass
{
    public abstract float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0);
}

public enum CalculationType { Add, Subtract, Multiply, Divide }

public class NoisePass_Static : NoisePass
{
    public float value;
    public CalculationType calcType;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        switch (calcType)
        {
            case CalculationType.Subtract:
                return noise - value;
            case CalculationType.Multiply:
                return noise * value;
            case CalculationType.Divide:
                return noise / value;
            case CalculationType.Add:
            default:
                return noise + value;
        }
        
    }
}

public class NoisePass_WeightedAveragePositionalChunkTerrainNoise : NoisePass
{
    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        int amt = 0; // This is twice the distance for 2d (4x for 3d?)
        float totalNoise = 0;
        foreach ((ChunkTile chunk, int distance) chunk in MapGenerationManager.Instance.GetNearbyChunks(position))
        {
            if (chunk.chunk == null) return noise;
            amt += chunk.distance;
            float chunkNoise = 0;
            foreach (NoisePass pass in chunk.chunk.terrainNoise)
            {
                chunkNoise = pass.NoiseMod(chunkNoise, position);
            }
            totalNoise += chunkNoise * chunk.distance;
        }
        return totalNoise / amt + noise;
    }
}

public class NoisePass_PositionAxisAmount : NoisePass
{
    public int axis;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        return noise + position[axis];
    }
}

public class NoisePass_GradientNoise1D : NoisePass
{
    public float scale;
    public Vector2 noiseRange;
    public CalculationType calcType;
    public int axis = 0;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        Random.InitState(seed);
        float randomOffset = Random.Range(-100000f, 100000f);
        float noiseValue = Mathf.Lerp(noiseRange.x, noiseRange.y, Noise.CalcPixel1D((int)(position[axis] + randomOffset), scale) / 255f);
        switch (calcType)
        {
            case CalculationType.Subtract:
                return noise - noiseValue;
            case CalculationType.Multiply:
                return noise * noiseValue;
            case CalculationType.Divide:
                return noise / noiseValue;
            case CalculationType.Add:
            default:
                return noise + noiseValue;
        }
    }
}

public class NoisePass_GradientNoise2D : NoisePass
{
    public float scale;
    public Vector2 noiseRange;
    public CalculationType calcType;
    public int axis1 = 0, axis2 = 2;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        Random.InitState(seed);
        Vector2 randomOffset = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));
        float noiseValue = Mathf.Lerp(noiseRange.x, noiseRange.y, Noise.CalcPixel2D((int)(position[axis1] + randomOffset.x), (int)(position[axis2] + randomOffset.y), scale) / 255f);
        switch (calcType)
        {
            case CalculationType.Subtract:
                return noise - noiseValue;
            case CalculationType.Multiply:
                return noise * noiseValue;
            case CalculationType.Divide:
                return noise / noiseValue;
            case CalculationType.Add:
            default:
                return noise + noiseValue;
        }
    }
}

public class NoisePass_GradientNoise3D : NoisePass
{
    public float scale;
    public Vector2 noiseRange;
    public CalculationType calcType;
    public int axis1 = 0, axis2 = 1, axis3 = 2;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        Random.InitState(seed);
        Vector3 randomOffset = new Vector3(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));
        float noiseValue = Mathf.Lerp(noiseRange.x, noiseRange.y, Noise.CalcPixel3D((int)(position[axis1] + randomOffset.x), (int)(position[axis2] + randomOffset.x), (int)(position[axis3] + randomOffset.x), scale) / 255f);
        switch (calcType)
        {
            case CalculationType.Subtract:
                return noise - noiseValue;
            case CalculationType.Multiply:
                return noise * noiseValue;
            case CalculationType.Divide:
                return noise / noiseValue;
            case CalculationType.Add:
            default:
                return noise + noiseValue;
        }
    }
}

public class NoisePass_Clamp : NoisePass
{
    public Vector2 clampRange;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        return Mathf.Clamp(noise, clampRange.x, clampRange.y);
    }
}

public class NoisePass_Scale : NoisePass
{
    public float center, scale;

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
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

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
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

    public override float NoiseMod(float noise, MultidimensionalPosition position, int seed = 0)
    {
        return noise - noise % step;
    }
}