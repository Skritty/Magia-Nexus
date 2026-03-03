using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapGenerator_EntropicPropagation : MapGenerator
{
    public float tileGenWaitTime;
    public bool startWithSeedTileInstead;
    public TileSuperposition seedTile;
    [SerializeReference]
    public List<GenerationRule> generationRules = new();
    [SerializeReference]
    public List<GenerationRule> propagationRules = new();
    private EntropicSet<MultidimensionalPosition> entropicMap = new();
    private Queue<MultidimensionalPosition> propagationQueue = new();

    public override IEnumerator Generate(MapGenerationManager manager,NTree<TileSuperposition> tree, Bounds generationBounds, Action<MultidimensionalPosition> generateTile)
    {
        entropicMap.Clear();
        if (startWithSeedTileInstead)
        {
            MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x), (ushort)(generationBounds.center.y), (ushort)(generationBounds.center.z));
            if (tree[position] != null)
            {
                if(tree[position].Entropy > 1)
                    entropicMap.Add(tree[position].Entropy, position);
            }
            else if (seedTile != null)
            {
                TileSuperposition tile = tree[position] = seedTile;
                entropicMap.Add(tile.Entropy, position);
            }
        }
        else
        {
            for (int x = (int)-generationBounds.extents.x; x < generationBounds.extents.x; x++)
            {
                for (int y = (int)-generationBounds.extents.y; y < generationBounds.extents.y; y++)
                {
                    for (int z = (int)-generationBounds.extents.z; z < generationBounds.extents.z; z++)
                    {
                        MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x + x), (ushort)(generationBounds.center.y + y), (ushort)(generationBounds.center.z + z));
                        TileSuperposition tile = tree[position];
                        if (tile != null && tile.Entropy != 0)
                        {
                            entropicMap.Add(tile.Entropy, position);
                        }
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
                foreach(MultidimensionalPosition newTilePosition in generationRule.Generate(manager, tree, position))
                {
                    if (!generationBounds.Contains(new Vector3(newTilePosition[0], newTilePosition[1], newTilePosition[2]))) continue;
                    entropicMap.Add(tree[newTilePosition].Entropy, newTilePosition);
                }
            }
            if (tree[position] == null) continue;

            if(propagationRules.Count > 0)
            {
                propagationQueue.Enqueue(position);
                while (propagationQueue.Count > 0)
                {
                    MultidimensionalPosition propagationPosition = propagationQueue.Dequeue();
                    foreach (GenerationRule propagationRule in propagationRules)
                    {
                        foreach (MultidimensionalPosition updatedTile in propagationRule.Generate(manager, tree, propagationPosition))
                        {
                            if (!generationBounds.Contains(new Vector3(updatedTile[0], updatedTile[1], updatedTile[2]))) continue;

                            if (!propagationQueue.Contains(updatedTile)) propagationQueue.Enqueue(updatedTile);

                            switch (tree[updatedTile].Entropy)
                            {
                                case 0:
                                    entropicMap.Remove(updatedTile);
                                    break;
                                case 1:
                                    entropicMap.Remove(updatedTile);
                                    generateTile.Invoke(updatedTile);
                                    break;
                                default:
                                    if (!entropicMap.Contains(updatedTile)) entropicMap.Add(tree[updatedTile].Entropy, updatedTile);
                                    else entropicMap.Update(tree[updatedTile].Entropy, updatedTile);
                                    break;
                            }
                        }
                    }
                }
            }

            /*if (tree[position].Solved) generatedTiles.Add(position);
            else if (generatedTiles.Contains(position)) generatedTiles.Remove(position);*/
            if (tree[position].Solved)
            {
                generateTile.Invoke(position);
                if(tileGenWaitTime != 0) yield return new WaitForSeconds(tileGenWaitTime);
            }
        }
    }

    public override void DrawGizmos(NTree<TileSuperposition> tree)
    {
        foreach (MultidimensionalPosition position in entropicMap.AsArray)
        {
            if(tree[position].Entropy > 1)
            {
                Gizmos.color = new Color(1, 1, 1, tree[position].Entropy / 3f);
                Gizmos.DrawCube(new Vector3(position[0], position[1], position[2]), Vector3.one);
            }
        }
    }
}

