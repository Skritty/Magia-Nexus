using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;

[System.Serializable]
public abstract class GenerationRule
{
    public abstract List<MultidimensionalPosition> Generate(MapGenerationManager manager /*TODO: THIS IS TEMP*/, NTree<TileSuperposition> tree, MultidimensionalPosition position);
}

public enum TileSolveType { Or, Not, And, Exclusively, OrIfEmpty }

public class GenerationRule_ChunkReference : GenerationRule
{
    [SerializeReference]
    public List<NoisePass> xVirtualOffset = new();
    [SerializeReference]
    public List<NoisePass> yVirtualOffset = new();
    [SerializeReference]
    public List<NoisePass> zVirtualOffset = new();
    public override List<MultidimensionalPosition> Generate(MapGenerationManager manager, NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        List<MultidimensionalPosition> generatedTiles = new();

        Vector3 virtualOffset = new();
        foreach (NoisePass pass in xVirtualOffset)
        {
            virtualOffset.x = pass.NoiseMod(virtualOffset.x, position, 0);
        }
        foreach (NoisePass pass in yVirtualOffset)
        {
            virtualOffset.y = pass.NoiseMod(virtualOffset.y, position, 1);
        }
        foreach (NoisePass pass in xVirtualOffset)
        {
            virtualOffset.z = pass.NoiseMod(virtualOffset.z, position, 2);
        }

        MultidimensionalPosition virtualPosition = new((ushort)(position[0] + virtualOffset.x), (ushort)(position[1] + virtualOffset.y), (ushort)(position[2] + virtualOffset.z));
        ChunkTile chunk = manager.GetChunk(virtualPosition);
        if (chunk == null) return generatedTiles;
        foreach (GenerationRule rule in chunk.generationRules)
        {
            generatedTiles.AddRange(rule.Generate(manager, tree, position));
        }
        return generatedTiles;
    }
}

public class GenerationRule_NoiseValueRange : GenerationRule
{
    public bool createNewTiles = true;
    [HideIf("@!createNewTiles")]
    public TileSuperposition defaultTile;
    public List<NoiseValueRange> tileSettings;
    [SerializeReference]
    public List<NoisePass> noisePasses = new();

    [Serializable]
    public struct NoiseValueRange
    {
        public TileSuperposition tileOptions;
        public Vector2 noiseRange;
        public TileSolveType solveType;
    }

    public override List<MultidimensionalPosition> Generate(MapGenerationManager manager, NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        List<MultidimensionalPosition> generatedTiles = new();
        float noise = 0;
        foreach (NoisePass pass in noisePasses)
        {
            noise = pass.NoiseMod(noise, position);
        }

        if (tree[position] == null)
        {
            if (!createNewTiles) return generatedTiles;
            tree[position] = new TileSuperposition(defaultTile);
            generatedTiles.Add(position);
        }

        if (tree[position].Solved) return generatedTiles;
        
        foreach (NoiseValueRange setting in tileSettings)
        {
            if (noise >= setting.noiseRange.x && noise <= setting.noiseRange.y)
            {
                switch (setting.solveType)
                {
                    case TileSolveType.Or:
                        tree[position].Or(setting.tileOptions);
                        break;
                    case TileSolveType.Not:
                        tree[position].And(setting.tileOptions.Not());
                        break;
                    case TileSolveType.And:
                        tree[position].And(setting.tileOptions);
                        break;
                    case TileSolveType.Exclusively:
                        {
                            tree[position].SetAll(false);
                            tree[position].Or(setting.tileOptions);
                            break;
                        }
                    case TileSolveType.OrIfEmpty:
                        {
                            if (tree[position].Entropy == 0)
                                tree[position].Or(setting.tileOptions);
                            break;
                        }
                }
            }
        }
        return generatedTiles;
    }
}

public class GenerationRule_WeightedCollapse : GenerationRule
{
    public List<TileWeight> tileSettings;
    private WeightedChance<TileSuperposition> potentialTiles = new();
    // Pair each tile with a collapse weight
    // Give each tile weight multiplier conditions
    //public NativeArray<bool> testNativeArray;
    [Serializable]
    public struct TileWeight
    {
        public TileSuperposition tileOptions;
        public float weight;
        
        //[SerializeReference]
        //public List<TileWeightModifier> weightModifiers;
    }

    public override List<MultidimensionalPosition> Generate(MapGenerationManager manager, NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        TileSuperposition tile = tree[position];
        if (tile == null) return new();
        potentialTiles.Clear();
        foreach (TileWeight tileWeight in tileSettings)
        {
            if (!tile.ContainsSubset(tileWeight.tileOptions)) continue;
            potentialTiles.Add(tileWeight.tileOptions, tileWeight.weight);
        }
        if (potentialTiles.TotalWeight == 0) return new();
        tree[position].SetAll(false);
        TileSuperposition chosen = potentialTiles.GetRandomEntry();
        tree[position].Or(chosen);
        return new();
    }
}

// Needs to add new/existing tiles to the generation sequence (if not already added)
public class GenerationRule_MapConstraints : GenerationRule
{
    public bool createNewTiles = true;
    [HideIf("@!createNewTiles")]
    public TileSuperposition defaultTile;
    public TileSolveType solveType;
    public List<Vector3> offsets;
    public List<ObservationRule> observationRules = new();
    [Serializable]
    public struct ObservationRule
    {
        public TileSuperposition tileReference, tileOptions;
    }
    // Figure out tile rotations

    public override List<MultidimensionalPosition> Generate(MapGenerationManager manager, NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        List<MultidimensionalPosition> modifiedTiles = new();
        TileSuperposition tile = tree[position];

        // Compile rules together
        TileSuperposition options = new TileSuperposition(tile.tileset.tiles.Count);
        foreach (ObservationRule rule in observationRules)
        {
            if (tile.ContainsSubset(rule.tileReference))
            {
                options.Or(rule.tileOptions);
            }
        }

        // Apply rule
        foreach (Vector3 offset in offsets)
        {
            MultidimensionalPosition offsetPosition = new MultidimensionalPosition((ushort)(position[0] + offset.x), (ushort)(position[1] + offset.y), (ushort)(position[2] + offset.z));
            if (offsetPosition[0] >= ushort.MaxValue || offsetPosition[1] >= ushort.MaxValue || offsetPosition[2] >= ushort.MaxValue) continue;

            TileSuperposition offsetTile = tree[offsetPosition];
            if (offsetTile == null)
            {
                if (!createNewTiles) continue;
                tree[offsetPosition] = offsetTile = new TileSuperposition(defaultTile);
                modifiedTiles.Add(offsetPosition);
            }
            //else if (offsetTile.generated) continue;

            int entropy = offsetTile.Entropy;
            switch (solveType)
            {
                case TileSolveType.Or:
                    offsetTile.Or(options);
                    break;
                case TileSolveType.Not:
                    offsetTile.And(options.Not());
                    break;
                case TileSolveType.And:
                    offsetTile.And(options);
                    break;
                case TileSolveType.Exclusively:
                    offsetTile.SetAll(false);
                    offsetTile.Or(options);
                    break;
            }
            
            if (entropy != offsetTile.Entropy && !modifiedTiles.Contains(offsetPosition)) modifiedTiles.Add(offsetPosition);
        }
        return modifiedTiles;
    }

    /*private void Collapse(NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        TileSuperposition tiles = tree[position];
        if (position[0] != ushort.MaxValue) Collapse(tiles, 0, new((ushort)(position[0] + 1), position[1], position[2]));
        if (position[0] != 0) Collapse(tiles, 1, new((ushort)(position[0] - 1), position[1], position[2]));
        if (position[1] != ushort.MaxValue) Collapse(tiles, 2, new(position[0], (ushort)(position[1] + 1), position[2]));
        if (position[1] != 0) Collapse(tiles, 3, new(position[0], (ushort)(position[1] - 1), position[2]));
        if (position[2] != ushort.MaxValue) Collapse(tiles, 4, new(position[0], position[1], (ushort)(position[2] + 1)));
        if (position[2] != 0) Collapse(tiles, 5, new(position[0], position[1], (ushort)(position[2] - 1)));
    }

    private ulong GetAllowedTiles(TileSuperposition tiles, int connectionIndex)
    {
        allowed = 0;
        foreach (WFCTile tile in tiles.GetObjects(tileSet.ToArray()))
        {
            allowed |= tile.connections[connectionIndex].allowedTiles;
        }
        return allowed;
    }

    private void Collapse(TileSuperposition possibleConnections, int connectionIndex, MultidimensionalPosition position)
    {
        TileSuperposition potentialTiles = spatialTree[position];
        // Is this tile outside of the map or is it already solved?
        if (potentialTiles == null)
        {
            potentialTiles = AddTile(position);
        }
        else if (potentialTiles.Solved) return;

        ulong allowedTiles = GetAllowedTiles(possibleConnections, connectionIndex);
        //updating = index;
        ulong options = potentialTiles.options;
        potentialTiles.options &= allowedTiles;

        if (options != potentialTiles.options)
        {
            if (potentialTiles.options == 0)
            {
                // Error! No tile options left to pick
                //GenerateTile(ErrorTile.subtiles[0, 0, 0], ErrorTile, position);
                generatedTiles.Add(position);
                entropicMap.Remove(position);
                return;
            }
            if (potentialTiles.Solved)
            {
                // Tile is determined
                //WFCTile tile = potentialTiles.GetObjects(tileOptions.ToArray())[0];
                //GenerateTile(tile, tileGroupsByUID[tile.groupUID], position);
                generatedTiles.Add(position);
                entropicMap.Remove(position);
            }
            else
            {
                // Tile entropy reduced
                entropicMap.Update(potentialTiles.Entropy, position);
            }

            // Add to the update queue
            if (!inQueue.Contains(position) && (IsGenerationTile(position) || IsBufferTile(position)))
            {
                updateQueue.Enqueue(position);
                inQueue.Add(position);
            }
        }
    }*/
}