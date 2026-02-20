using System;
using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Utilities;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[Serializable]
public class MapGenerator_Entropic : MapGenerator
{
    [SerializeReference]
    public List<GenerationRule> generationRules = new();
    private EntropicSet<MultidimensionalPosition> entropicMap = new();

    public override HashSet<MultidimensionalPosition> Generate(NTree<TileSuperposition> tree, Bounds generationBounds, Dictionary<string, WFCTileGroup> tilesByGroupUID)
    {
        HashSet<MultidimensionalPosition> generatedTiles = new();
        entropicMap.Clear();
        for (int x = (int)-generationBounds.extents.x; x < generationBounds.extents.x; x++)
        {
            for (int y = (int)-generationBounds.extents.y; y < generationBounds.extents.y; y++)
            {
                for (int z = (int)-generationBounds.extents.z; z < generationBounds.extents.z; z++)
                {
                    MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x + x), (ushort)(generationBounds.center.y + y), (ushort)(generationBounds.center.z + z));
                    TileSuperposition tile = tree[position];
                    if (tile != null && !tile.generated && tile.Entropy != 0 && !tile.Solved)
                    {
                        entropicMap.Add(tile.Entropy, position);
                    }
                }
            }
        }

        while (entropicMap.Count > 0)
        {
            MultidimensionalPosition position = entropicMap.GetRandomAtLowestEntropy();
            entropicMap.Remove(position);

            foreach (GenerationRule generationRule in generationRules)
            {
                if (!generationRule.Generate(tree, position)) break;
            }
            if (tree[position] == null) continue;
            if (tree[position].Solved) generatedTiles.Add(position);
            else if (generatedTiles.Contains(position)) generatedTiles.Remove(position);
        }

        return generatedTiles;
    }
}

