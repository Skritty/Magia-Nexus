using System;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using UnityEngine;

[Serializable]
public class WFCMapGenerator : MapGenerator
{
    //public List<WFCTileGroup> tileGroupPrefabs;
    //public float creationDelay, solveDelay;
    public int buffer;

    private NTree<FlagList> spatialTree;
    private List<WFCTile> tileSet;
    private EntropicSet<MultidimensionalPosition> entropicMap;
    private Queue<MultidimensionalPosition> updateQueue = new();
    private HashSet<MultidimensionalPosition> inQueue = new();

    private Bounds generationBounds, bufferBounds;
    private FlagList tileOptions;
    private ulong defaultTileFlags;
    private WeightedChance<WFCTile> potentialTiles = new();
    private List<MultidimensionalPosition> generatedTiles = new();
    private ulong allowed;
    private const ulong ULOne = 1;

    /*public override void Initialize(FlagList tileOptions)
    {
        //spatialTree = new();

        this.tileOptions = tileOptions;
        tileGroupsByUID = new();
        tileGroupsByUID.Add(ErrorTile.groupUID, ErrorTile);

        // Add all valid tiles in each group to the list of possible tiles.
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            tileGroupsByUID.Add(tileGroup.groupUID, tileGroup);
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (!tile.isHole)
                {
                    tileOptions.Add(tile);
                }
            }
        }

        entropicMap = new(tileOptions.Count);
        defaultTileFlags = ulong.MaxValue >> Mathf.Clamp(64 - tileOptions.Count, 0, 64);

        targetCenter = center.position - transform.position;
    }*/

    public override List<MultidimensionalPosition> Generate(NTree<FlagList> spatialTree, Bounds generationBounds, List<WFCTile> tileSet)
    {
        this.spatialTree = spatialTree;
        this.tileSet = tileSet;
        this.generationBounds = generationBounds;
        bufferBounds = generationBounds;
        bufferBounds.Expand(buffer);
        generatedTiles.Clear();

        entropicMap = new(tileSet.Count);
        defaultTileFlags = ulong.MaxValue >> Mathf.Clamp(64 - tileSet.Count, 0, 64);

        for (int x = (int)-generationBounds.extents.x; x < generationBounds.extents.x; x++)
        {
            for (int y = (int)-generationBounds.extents.y; y < generationBounds.extents.y; y++)
            {
                for (int z = (int)-generationBounds.extents.z; z < generationBounds.extents.z; z++)
                {
                    MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x + x), (ushort)(generationBounds.center.y + y), (ushort)(generationBounds.center.z + z));
                    FlagList tile = spatialTree[position];
                    if (tile.Entropy != 0 && !tile.Solved)
                    {
                        entropicMap.Add(spatialTree[position].Entropy, position);
                    }
                }
            }
        }

        // Solve the WFC
        while (entropicMap.Count > 0)
        {
            MultidimensionalPosition position = entropicMap.GetRandomAtLowestEntropy();
            entropicMap.Remove(position);

            potentialTiles.Clear();
            foreach (WFCTile tile in spatialTree[position].GetObjects(tileSet.ToArray()))
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();
            generatedTiles.Add(position);
            spatialTree[position].options = ULOne << tileSet.IndexOf(selectedTile);
            updateQueue.Enqueue(position);
            inQueue.Add(position);
            while (updateQueue.Count > 0)
            {
                position = updateQueue.Dequeue();
                inQueue.Remove(position);
                Observe(position);
            }
        }
        return generatedTiles;
    }

    private FlagList AddTile(MultidimensionalPosition position)
    {
        FlagList flagList = new(defaultTileFlags);
        spatialTree.TryAddData(flagList, position, out _);
        if (IsGenerationTile(position))
        {
            entropicMap.Add(tileSet.Count, position);
        }
        return flagList;
    }

    /*private void FixedUpdate()
    {
        if (!generating)
        {
            targetCenter = Vector3.Lerp(targetCenter, center.position - transform.position, 0.01f);
            
            if(Vector3.Distance(previousCenter, targetCenter) >= 1)
            {
                StartCoroutine(GenerateMap());
                previousCenter = targetCenter;
            }
        }
    }*/



    /*public IEnumerator GenerateMap()//byte targetDepth, Bounds generationBounds, int buffer)
    {
        generating = true;

        if (center.x - transform.position.x < 0
            || center.y - transform.position.y < 0
            || center.z - transform.position.z < 0) return null;

        MultidimensionalPosition generationStartPos = new(
            (ushort)(center.x - transform.position.x),
            (ushort)(center.y - transform.position.y),
            (ushort)(center.z - transform.position.z));

        MultidimensionalPosition generationStartPos = new(
           (ushort)(targetCenter.x),
           (ushort)(targetCenter.y),
           (ushort)(targetCenter.z));

        // 1000 = 8
        // 1100 = 12

        // Set up seed nodes
        if (spatialTree[generationStartPos] == null)//.GetDataAtPosition(generationStartPos, targetDepth) == null)
        {
            AddTile(generationStartPos);
        }
        else
        {
            foreach (MultidimensionalPosition position in bufferTiles.ToArray())
            {
                if (!IsGenerationTile(position)) continue;
                entropicMap.Add(spatialTree[position].Entropy, position);
                bufferTiles.Remove(position);
            }
        }

        int tilesPerDelay = 25;
        int tilesPlaced = 0;
        // Solve the WFC
        while (entropicMap.Count > 0)
        {
            MultidimensionalPosition position = entropicMap.GetRandomAtLowestEntropy();
            entropicMap.Remove(position);

            potentialTiles.Clear();
            foreach (WFCTile tile in spatialTree[position].GetObjects(tileOptions.ToArray()))
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();

            spatialTree[position].options = ULOne << tileOptions.IndexOf(selectedTile);
            updateQueue.Enqueue(position);
            inQueue.Add(position);
            if(GenerateTile(selectedTile, tileGroupsByUID[selectedTile.groupUID], position))
            {
                tilesPlaced++;
                if (tilesPlaced >= tilesPerDelay && creationDelay > 0)
                {
                    tilesPlaced = 0;
                    yield return new WaitForSeconds(creationDelay);
                }
            }
            while (updateQueue.Count > 0)
            {
                position = updateQueue.Dequeue();
                inQueue.Remove(position);
                Observe(position);
                if (solveDelay > 0) yield return new WaitForSeconds(solveDelay);
            }
        }

        // Instantiate tile visuals
        for (int x = 0; x < mapRepresentation.x; x++)
        {
            for (int y = 0; y < mapRepresentation.y; y++)
            {
                for (int z = 0; z < mapRepresentation.z; z++)
                {
                    if (mapRepresentation[x, y, z].Count != 1) continue;
                    WFCTile tile = mapRepresentation[x, y, z].First();
                    WFCTileGroup group = tileGroupsByUID[tile.groupUID];
                    if (tile.content == null) continue;
                    GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
                    visuals.transform.position += transform.position + new Vector3(x, y, z)
                        - group.bounds.center + group.bounds.extents;
                    visuals.transform.parent = transform;
                }
            }
        }

        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            Destroy(tileGroup.gameObject);
        }
        tileGroups.Clear();
        generating = false;
        //Debug.Break();
    }*/

    private bool IsBufferTile(MultidimensionalPosition position)
    {
        //float sqDist = Mathf.Pow(targetCenter.x - position[0], 2) + Mathf.Pow(targetCenter.y - position[1], 2) + Mathf.Pow(targetCenter.z - position[2], 2);
        //return sqDist > sqRadius && sqDist <= sqRadius + sqBufferSize;
        return bufferBounds.Contains(position.ToVector3) && !generationBounds.Contains(position.ToVector3);
    }

    private bool IsGenerationTile(MultidimensionalPosition position)
    {
        //float sqDist = Mathf.Pow(targetCenter.x - position[0], 2) + Mathf.Pow(targetCenter.y - position[1], 2) + Mathf.Pow(targetCenter.z - position[2], 2);
        //return sqDist <= sqRadius;
        return generationBounds.Contains(position.ToVector3);
    }

    private void Observe(MultidimensionalPosition position)
    {
        FlagList tiles = spatialTree[position];
        if (position[0] != ushort.MaxValue) Collapse(tiles, 0, new((ushort)(position[0] + 1), position[1], position[2]));
        if (position[0] != 0) Collapse(tiles, 1, new((ushort)(position[0] - 1), position[1], position[2]));
        if (position[1] != ushort.MaxValue) Collapse(tiles, 2, new(position[0], (ushort)(position[1] + 1), position[2]));
        if (position[1] != 0) Collapse(tiles, 3, new(position[0], (ushort)(position[1] - 1), position[2]));
        if (position[2] != ushort.MaxValue) Collapse(tiles, 4, new(position[0], position[1], (ushort)(position[2] + 1)));
        if (position[2] != 0) Collapse(tiles, 5, new(position[0], position[1], (ushort)(position[2] - 1)));
    }

    private ulong GetAllowedTiles(FlagList tiles, int connectionIndex)
    {
        allowed = 0;
        foreach (WFCTile tile in tiles.GetObjects(tileSet.ToArray()))
        {
            allowed |= tile.connections[connectionIndex].allowedTiles;
        }
        return allowed;
    }

    private void Collapse(FlagList possibleConnections, int connectionIndex, MultidimensionalPosition position)
    {
        FlagList potentialTiles = spatialTree[position];
        // Is this tile outside of the map or is it already solved?
        if (potentialTiles == null)
        {
            potentialTiles = AddTile(position);
        }
        else if(potentialTiles.Solved) return;

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
    }

    /*private bool GenerateTile(WFCTile tile, WFCTileGroup group, MultidimensionalPosition position)
    {
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Add(tile);

        if (tile.content == null) return false;
        GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        visuals.transform.position += transform.position + position.ToVector3
            - group.bounds.center + group.bounds.extents;
        visuals.transform.parent = transform;
        generatedTiles.Add(position);
        return true;
    }*/

    /*private void OnDrawGizmosSelected()
    {
        if (spatialTree == null || tileGroups == null) return;
        foreach (var node in spatialTree.nodes)
        {
            MultidimensionalPosition position = node.position;
            FlagList tiles = spatialTree[position];
            if (updating.Item1 == position[0] && updating.Item2 == position[1] && updating.Item3 == position[2])
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(position.ToVector3 + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
            }
            //else
            {
                //if (!IsGenerationTile(position)) continue;
                if (tiles.Solved)
                {
                    Gizmos.color = Color.blue;
                    continue;
                }
                else
                {
                    float a = (1f - tiles.Entropy * 1f / tileCount);
                    if (a == 0) continue;
                    Gizmos.color = new Color(a, a, a, a * 0.2f);
                }

                Gizmos.DrawCube(new Vector3(position[0], position[1], position[2]) + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
            }
        }
    }*/
}

