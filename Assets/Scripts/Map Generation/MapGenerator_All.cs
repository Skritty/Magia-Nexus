using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapGenerator_All : MapGenerator
{
    public float tileGenWaitTime;
    public bool skipNull;
    [SerializeReference]
    public List<GenerationRule> generationRules = new();
    public override IEnumerator Generate(MapGenerationManager manager,NTree<TileSuperposition> tree, Bounds generationBounds, Action<MultidimensionalPosition> generateTile)
    {
        //HashSet<MultidimensionalPosition> generatedTiles = new();
        for (int x = (int)-generationBounds.extents.x; x < generationBounds.extents.x; x++)
        {
            for (int y = (int)-generationBounds.extents.y; y < generationBounds.extents.y; y++)
            {
                for (int z = (int)-generationBounds.extents.z; z < generationBounds.extents.z; z++)
                {
                    MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x + x), (ushort)(generationBounds.center.y + y), (ushort)(generationBounds.center.z + z));
                    if ((tree[position] == null && skipNull) || tree[position] != null && tree[position].generated) continue;

                    foreach (GenerationRule generationRule in generationRules)
                    {
                        generationRule.Generate(manager, tree, position);
                    }
                    if (tree[position] == null) continue;
                    /*if (tree[position].Solved) generatedTiles.Add(position);
                    else if (generatedTiles.Contains(position)) generatedTiles.Remove(position);*/
                    if (tree[position].Solved)
                    {
                        generateTile.Invoke(position);
                        if (tileGenWaitTime != 0) yield return new WaitForSeconds(tileGenWaitTime);
                    }
                }
            }
        }
        yield return new WaitForSeconds(tileGenWaitTime);
        //return generatedTiles;
    }
}
