using System.Collections.Generic;
using UnityEngine;
public class MapGenerator_All : MapGenerator
{
    public bool skipNull;
    [SerializeReference]
    public List<GenerationRule> generationRules = new();
    public override HashSet<MultidimensionalPosition> Generate(NTree<TileSuperposition> tree, Bounds generationBounds, Dictionary<string, WFCTileGroup> tileGroupsByGroupUID)
    {
        HashSet<MultidimensionalPosition> generatedTiles = new();
        for (float x = generationBounds.center.x - generationBounds.extents.x; x < generationBounds.center.x + generationBounds.extents.x; x++)
        {
            for (float y = generationBounds.center.y - generationBounds.extents.y; y < generationBounds.center.y + generationBounds.extents.y; y++)
            {
                for (float z = generationBounds.center.z - generationBounds.extents.z; z < generationBounds.center.z + generationBounds.extents.z; z++)
                {
                    MultidimensionalPosition position = new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z);
                    if ((tree[position] == null && skipNull) || tree[position] != null && tree[position].generated) continue;

                    foreach (GenerationRule generationRule in generationRules)
                    {
                        if (!generationRule.Generate(tree, position)) break;
                    }
                    if (tree[position] == null) continue;
                    if (tree[position].Solved) generatedTiles.Add(position);
                    else if (generatedTiles.Contains(position)) generatedTiles.Remove(position);
                }
            }
        }
        return generatedTiles;
    }
}
