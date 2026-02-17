using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;
public class MapGenerator_GradientNoise : MapGenerator
{
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
        public int axis1 = 0, axis2 = 1;
        public Vector2 noiseRange;

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
        public int axis1 = 0, axis2 = 1, axis3 = 2;
        public Vector2 noiseRange;

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

    [System.Serializable]
    public class GenerationRule
    {
        public WFCTileGroup tileGroup;
        [System.NonSerialized]
        public List<WFCTile> tileSet = new();
        public Vector2 generationRange;
    }

    [SerializeReference]
    public List<NoisePass> noisePasses = new();
    public bool overwriteSolvedTiles, doNotCreateNewTiles, solveTilesDown;
    public List<GenerationRule> generationRules;
    public override HashSet<MultidimensionalPosition> Generate(NTree<TileSuperposition> spatialTree, Bounds generationBounds, Dictionary<string, WFCTileGroup> tileGroupsByGroupUID)
    {
        foreach (GenerationRule generationRule in generationRules)
        {
            if (generationRule.tileSet.Count > 0) break;
            foreach (WFCTile tile in tileGroupsByGroupUID[generationRule.tileGroup.groupUID].subtiles)
            {
                generationRule.tileSet.Add(tile);
            }
        }
        HashSet<MultidimensionalPosition> generatedTiles = new();
        List<WFCTile> tiles = new();
        for (float x = generationBounds.center.x - generationBounds.extents.x; x < generationBounds.center.x + generationBounds.extents.x; x++)
        {
            for (float y = generationBounds.center.y - generationBounds.extents.y; y < generationBounds.center.y + generationBounds.extents.y; y++)
            {
                for (float z = generationBounds.center.z - generationBounds.extents.z; z < generationBounds.center.z + generationBounds.extents.z; z++)
                {
                    MultidimensionalPosition position = new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z);
                    TileSuperposition tileOptions = spatialTree[position];
                    if (tileOptions != null && (tileOptions.generated || (!overwriteSolvedTiles && tileOptions.Solved))) continue;
                    
                    float noise = 0;
                    foreach (NoisePass pass in noisePasses)
                    {
                        noise = pass.NoiseMod(noise, position);
                    }

                    tiles.Clear();
                    foreach (GenerationRule generationRule in generationRules)
                    {
                        foreach (WFCTile tile in generationRule.tileSet)
                        {
                            if (noise >= generationRule.generationRange.x && noise <= generationRule.generationRange.y)
                            {
                                tiles.Add(tile);
                            }
                        }
                    }

                    if (tiles.Count > 0)
                    {
                        if (tileOptions == null)
                        {
                            if (doNotCreateNewTiles) continue;
                            tileOptions = new TileSuperposition(0);
                            spatialTree.TryAddData(tileOptions, position, out _);
                        }
                        foreach (WFCTile tile in tiles)
                        {
                            if (solveTilesDown)
                            {
                                tileOptions.options &= tile.tileBit;
                            }
                            else
                            {
                                tileOptions.options |= tile.tileBit;
                            }
                        }

                        if (tileOptions.Solved) generatedTiles.Add(position);
                        else if (generatedTiles.Contains(position)) generatedTiles.Remove(position);
                    }
                }
            }
        }
        return generatedTiles;
    }
}
