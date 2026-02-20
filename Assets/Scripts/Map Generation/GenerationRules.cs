using System;
using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Utilities;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class GenerationRule
{
    public abstract bool Generate(NTree<TileSuperposition> tree, MultidimensionalPosition position);
}

public enum TileSolveType { Or, Not, And, Exclusively, OrIfEmpty }

public class GenerationRule_NoiseValueRange : GenerationRule
{
    public List<NoiseValueRange> tileSettings;
    [SerializeReference]
    public List<NoisePass> noisePasses = new();
    public bool createNewTiles = true;

    [Serializable]
    public struct NoiseValueRange
    {
        public TileSuperposition tileOptions;
        public Vector2 noiseRange;
        public TileSolveType solveType;
    }

    public override bool Generate(NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        float noise = 0;
        foreach (NoisePass pass in noisePasses)
        {
            noise = pass.NoiseMod(noise, position);
        }

        if (tree[position] == null)
        {
            if (!createNewTiles) return true;
            tree[position] = new TileSuperposition();
        }
        
        foreach (NoiseValueRange setting in tileSettings)
        {
            if (noise >= setting.noiseRange.x && noise <= setting.noiseRange.y)
            {
                setting.tileOptions.Set(); // Remove this
                switch (setting.solveType)
                {
                    case TileSolveType.Or:
                        tree[position].options.Or(setting.tileOptions.options);
                        break;
                    case TileSolveType.Not:
                        tree[position].options.And(setting.tileOptions.options.Not());
                        break;
                    case TileSolveType.And:
                        tree[position].options.And(setting.tileOptions.options);
                        break;
                    case TileSolveType.Exclusively:
                        tree[position].options.SetAll(false);
                        tree[position].options.Or(setting.tileOptions.options);
                        break;
                    case TileSolveType.OrIfEmpty:
                        if(tree[position].Entropy == 0)
                            tree[position].options.Or(setting.tileOptions.options);
                        break;
                }
            }
        }
        return true;
    }
}

public class GenerationRule_WeightedCollapse : GenerationRule
{
    public List<TileWeight> tileSettings;
    private WeightedChance<TileSuperposition> potentialTiles = new();
    // Pair each tile with a collapse weight
    // Give each tile weight multiplier conditions
    public NativeArray<bool> testNativeArray;
    public struct TileWeight
    {
        public TileSuperposition tileOptions;
        public float weight;
        
        //[SerializeReference]
        //public List<TileWeightModifier> weightModifiers;
    }

    public override bool Generate(NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        potentialTiles.Clear();
        foreach (TileWeight tileWeight in tileSettings)
        {
            potentialTiles.Add(tileWeight.tileOptions, tileWeight.weight);
        }
        if (potentialTiles.TotalWeight == 0) return false;
        tree[position].options.SetAll(false);
        tree[position].options.Or(potentialTiles.GetRandomEntry().options);
        return true;
    }
}

public abstract class MapConstraint
{
    public TileSuperposition tileReference, tileOptions;
    public bool Applicable(TileSuperposition tile) => tile.Contains(tileReference);
    public abstract void ApplyConstraint(NTree<TileSuperposition> tree, MultidimensionalPosition position);
}

public class MapConstraint_PositionalConstraint : MapConstraint
{
    public TileSolveType solveType;
    public List<MultidimensionalPosition> offsets;
    public override void ApplyConstraint(NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        foreach(MultidimensionalPosition offset in offsets)
        {
            switch (solveType)
            {
                case TileSolveType.Or:
                    tree[position + offset].options.Or(tileOptions.options);
                    break;
                case TileSolveType.Not:
                    tree[position + offset].options.And(tileOptions.options);
                    break;
                case TileSolveType.And:
                    tree[position + offset].options.And(tileOptions.options);
                    break;
                case TileSolveType.Exclusively:
                    tree[position + offset].options.SetAll(false);
                    tree[position + offset].options.Or(tileOptions.options);
                    break;
            }
        }
    }
}

// Needs to add new/existing tiles to the generation sequence (if not already added)
public class GenerationRule_MapConstraints : GenerationRule
{

    public List<MapConstraint> constraints = new();
    // Pair each tile with a list of constraints they apply to the map
    // Figure out tile rotations

    public override bool Generate(NTree<TileSuperposition> tree, MultidimensionalPosition position)
    {
        TileSuperposition tile = tree[position];
        foreach (MapConstraint constraint in constraints)
        {
            if(!constraint.Applicable(tile)) continue;
            constraint.ApplyConstraint(tree, position);
        }
        return true;
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