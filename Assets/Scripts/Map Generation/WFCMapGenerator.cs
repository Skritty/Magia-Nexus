using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize, bufferSize;
    public List<WFCTileGroup> tileGroupPrefabs;
    [NonSerialized]
    public ThreeDimensionalSpatialRepresentation<List<WFCTile>> mapRepresentation;
    public float creationDelay, solveDelay;
    public WFCTileGroup ErrorTile;

    private EntropicSet<(int, int, int)> entropicMap;
    private Queue<(int, int, int)> updateQueue = new();
    private HashSet<(int, int, int)> inQueue = new();
    private (int, int, int) updating;

    [SerializeField]
    private List<WFCTileGroup> tileGroups = new();
    private Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
    private int tileCount;

    private List<WFCTile> GetMapTiles((int, int, int) index)
    {
        if ((uint)index.Item1 >= mapRepresentation.x || (uint)index.Item2 >= mapRepresentation.y || (uint)index.Item3 >= mapRepresentation.z) return null;
        return mapRepresentation[index.Item1, index.Item2, index.Item3];
    }

    private void Start()
    {
        SolveConnections();
        GenerateMap(xSize, ySize, zSize);
        //StartCoroutine(GenerateMap(xSize, ySize, zSize));
    }

    //[InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed"), Button("Solve Connections")]
    public void SolveConnections()
    {
        Profiler.BeginSample("Solve Connections");
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        tileGroups.Clear();
        tileGroupsByUID = new();
        List<WFCTile> allTiles = new List<WFCTile>();

        // Create prefab instances, put them into UID dictionary
        foreach (WFCTileGroup groupPrefab in tileGroupPrefabs)
        {
            WFCTileGroup loadedTileGroup = Instantiate(groupPrefab);
            loadedTileGroup.gameObject.SetActive(false);
            loadedTileGroup.transform.parent = transform;
            tileGroups.Add(loadedTileGroup);
            tileGroupsByUID.Add(loadedTileGroup.groupUID, loadedTileGroup);
            foreach (WFCTile tile in loadedTileGroup.subtiles)
            {
                allTiles.Add(tile);
                tileCount++;
            }
        }

        // Set up connections with loaded tile groups
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            // Handle internal connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (tile.weight == 0) tile.weight = tileGroup.weight;

                WFCTile[] adjecentTiles = tileGroup.subtiles.GetAdjecentObjects(tile);
                for (int i = 0; i < 6; i++)
                {
                    if (adjecentTiles[i] == null) continue;
                    if (adjecentTiles[i].isHole)
                    {
                        tile.connections[i].allowedTileRefs.AddRange(adjecentTiles[i].holeAllowedTileRefs);
                        tile.connections[i].isInternalConnection = false;
                    }
                    else
                    {
                        tile.connections[i].allowedTiles.Add(adjecentTiles[i]);
                        tile.connections[i].isInternalConnection = true;
                    }
                }
            }

            // Handle group links
            /*foreach (WFCTile tile in tileGroup.subtiles)
            {
                for (int i = 0; i < 6; i++)
                {
                    foreach (WFCTileRef connectedTileRef in tile.connections[i].allowedTileRefs.ToArray())
                    {
                        if (!tileGroupsByUID.ContainsKey(connectedTileRef.groupUID)) continue;
                        // Find the index where the connecting tile would be
                        (int, int, int) projectedIndex = tileGroup.subtiles.GetIndex(tile);
                        projectedIndex = (projectedIndex.Item1 + (i == 0 || i == 1 ? -(i % 2 * 2 - 1) : 0),
                            projectedIndex.Item2 + (i == 2 || i == 3 ? -(i % 2 * 2 - 1) : 0),
                            projectedIndex.Item3 + (i == 4 || i == 5 ? -(i % 2 * 2 - 1) : 0));
                        ConjoinTileGroups(tileGroup, tileGroupsByUID[connectedTileRef.groupUID], projectedIndex, (connectedTileRef.x, connectedTileRef.y, connectedTileRef.z));
                    }
                }
            }*/

            // Handle connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (tile.isHole) continue;
                // Set up and reciprocate actual tile connections from tileRefs
                for (int i = 0; i < 6; i++)
                {
                    if (tile.connections[i].isInternalConnection) continue;
                    int reciprocatedIndex = i + (i % 2 > 0 ? -1 : 1);
                    if (tile.connections[i].allowedTileRefs.Count == 0)
                    {
                        // Wildcard tile
                        foreach(WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            if (!tile.connections[i].allowedTiles.Contains(tileActual))
                                tile.connections[i].allowedTiles.Add(tileActual);
                            if (!otherConnection.allowedTiles.Contains(tile))
                                otherConnection.allowedTiles.Add(tile);
                        }
                    }
                    else
                    {
                        foreach (WFCTileRef connectedTileRef in tile.connections[i].allowedTileRefs)
                        {
                            if (!tileGroupsByUID.ContainsKey(connectedTileRef.groupUID))
                            {
                                // Tile group referened doesn't exist in this generation
                                continue;
                            }
                            WFCTile tileActual = tileGroupsByUID[connectedTileRef.groupUID].subtiles[connectedTileRef.x, connectedTileRef.y, connectedTileRef.z];
                            if (!tile.connections[i].allowedTiles.Contains(tileActual))
                                tile.connections[i].allowedTiles.Add(tileActual);
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (!otherConnection.allowedTiles.Contains(tile))
                                otherConnection.allowedTiles.Add(tile);
                        }
                    }
                }
            }
        }
        Profiler.EndSample();
    }

    private void ConjoinTileGroups(WFCTileGroup group1, WFCTileGroup group2, (int, int, int) projectedIndex, (int, int, int) initialConnectionIndex)
    {
        // Iterate through group1 subtiles (plus 1 around)
        for (int x = -1; x <= group1.subtiles.x; x++)
        {
            for (int y = -1; y <= group1.subtiles.y; y++)
            {
                for (int z = -1; z <= group1.subtiles.z; z++)
                {
                    (int, int, int) connectionIndex = (
                        initialConnectionIndex.Item1 - projectedIndex.Item1 + x,
                        initialConnectionIndex.Item2 - projectedIndex.Item2 + y,
                        initialConnectionIndex.Item3 - projectedIndex.Item3 + z);

                    if ((uint)connectionIndex.Item1 >= group2.subtiles.x
                        || (uint)connectionIndex.Item2 >= group2.subtiles.y
                        || (uint)connectionIndex.Item3 >= group2.subtiles.z)
                        continue;

                    if(group2.subtiles[connectionIndex].isHole) continue;

                    WFCTile selfTile = null;
                    if ((uint)x < group1.subtiles.x && (uint)y < group1.subtiles.y && (uint)z < group1.subtiles.z)
                    {
                        selfTile = group1.subtiles[x, y, z];
                    }

                    if (selfTile == null || selfTile.isHole)
                    {
                        WFCTile[] holeAdjacentTiles = group1.subtiles.GetAdjecentObjects((x, y, z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i] == null || holeAdjacentTiles[i].isHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            WFCTileRef tileRef = new WFCTileRef(group2.groupUID, connectionIndex.Item1, connectionIndex.Item2, connectionIndex.Item3);
                            if (!connection.allowedTileRefs.Any(x => x.groupUID == tileRef.groupUID)) connection.allowedTileRefs.Add(tileRef);

                            //Debug.Log($"Connecting {group2.gameObject.name} ({projectedIndex}) to {gameObject.name} ({initialConnectionIndex}) at connection index {i + (i % 2 > 0 ? -1 : 1)} | ");
                            // xyz = (0,0,-1), initialSelfIndex = (0,0,-1)
                            // initialConnectionIndex = (0,0,0), goalIndex = (1,0,0)
                            // initialConnectionIndex - initialSelfIndex + xyz = goalIndex
                        }
                    }
                    else
                    {
                        // Overlaps!
                        Debug.LogWarning($"{gameObject.name} overlaps with connection {group2.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }

    public void GenerateMap(int xSize, int ySize, int zSize)
    {
        List<WFCTile> tiles = new();
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
                    tiles.Add(tile);
                }
            }
        }

        // Setup the initial "waveform"
        mapRepresentation = new(xSize + 2 * bufferSize, ySize + 2 * bufferSize, zSize + 2 * bufferSize);
        entropicMap = new(tiles.Count);
        for (int x = 0; x < mapRepresentation.x; x++)
        {
            for (int y = 0; y < mapRepresentation.y; y++)
            {
                for (int z = 0; z < mapRepresentation.z; z++)
                {
                    mapRepresentation[x, y, z] = new List<WFCTile>();
                    mapRepresentation[x, y, z].AddRange(tiles);
                    (int, int, int) index = (x, y, z);
                    if (!IsBufferTile(index))
                    {
                        entropicMap.Add(tiles.Count, index);
                    }
                }
            }
        }
        // Solve the WFC
        while (entropicMap.Count > 0)
        {
            (int, int, int) index = entropicMap.GetRandomAtLowestEntropy();
            WeightedChance<WFCTile> potentialTiles = new();
            List<WFCTile> tiles2 = mapRepresentation[index.Item1, index.Item2, index.Item3];
            foreach (WFCTile tile in mapRepresentation[index.Item1, index.Item2, index.Item3])
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();
            if (!inQueue.Contains(index))
            {
                updateQueue.Enqueue(index);
                inQueue.Add(index);
            }
            GenerateTile(selectedTile, tileGroupsByUID[selectedTile.groupUID], index);
            while (updateQueue.Count > 0)
            {
                (int, int, int) updatePosition = updateQueue.Dequeue();
                inQueue.Remove(updatePosition);
                Observe(updatePosition.Item1, updatePosition.Item2, updatePosition.Item3);
                //if (solveDelay > 0) yield return new WaitForSeconds(solveDelay);
            }
            //if (creationDelay > 0) yield return new WaitForSeconds(creationDelay);
        }

        // Instantiate tile visuals
        for (int x = 0; x < mapRepresentation.x; x++)
        {
            for (int y = 0; y < mapRepresentation.y; y++)
            {
                for (int z = 0; z < mapRepresentation.z; z++)
                {
                    if (mapRepresentation[x, y, z].Count != 1) continue;
                    WFCTile tile = mapRepresentation[x, y, z][0];
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
        Debug.Break();
    }

    private bool IsBufferTile((int, int, int) index)
    {
        if ((uint)(index.Item1 - bufferSize) >= xSize) return true;
        if ((uint)(index.Item2 - bufferSize) >= ySize) return true;
        if ((uint)(index.Item3 - bufferSize) >= zSize) return true;
        return false;
    }

    private void Observe(int x, int y, int z)
    {
        Profiler.BeginSample("Observe");
        List<WFCTile> tiles = mapRepresentation[x, y, z];
        Collapse(tiles, 0, (x + 1, y, z));
        Collapse(tiles, 1, (x - 1, y, z));
        Collapse(tiles, 2, (x, y + 1, z));
        Collapse(tiles, 3, (x, y - 1, z));
        Collapse(tiles, 4, (x, y, z + 1));
        Collapse(tiles, 5, (x, y, z - 1));
        Profiler.EndSample();
    }

    private List<WFCTile> GetAllowedTiles(List<WFCTile> tiles, int connectionIndex)
    {
        List<WFCTile> allowed = new();
        foreach (WFCTile tile in tiles)
        {
            if(tile.connections[connectionIndex].allowedTiles.Count == 0)
            {
                allowed.Clear();
                break;
            }
            allowed.AddRange(tile.connections[connectionIndex].allowedTiles);
        }
        return allowed;
    }

    private void Collapse(List<WFCTile> possibleConnections, int connectionIndex, (int, int, int) index)
    {
        List<WFCTile> potentialTiles = GetMapTiles(index);
        // Is this tile outside of the map?
        if (potentialTiles == null) return;

        // Does this tile have yet to be solved?
        if (potentialTiles.Count <= 1) return;

        // Is this connection a wildcard?
        List<WFCTile> allowedTiles = GetAllowedTiles(possibleConnections, connectionIndex);
        if (allowedTiles.Count == 0) return;

        updating = index;

        int removed = potentialTiles.RemoveAll(x => !allowedTiles.Contains(x));

        if (removed > 0)
        {
            if (potentialTiles.Count == 0)
            {
                // Error! No tile options left to pick
                //GenerateTile(ErrorTile.subtiles[0, 0, 0], ErrorTile, index);
                mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
                mapRepresentation[index.Item1, index.Item2, index.Item3].Add(ErrorTile.subtiles[0, 0, 0]);
                entropicMap.Remove(index);
                return;
            }

            if (potentialTiles.Count == 1)
            {
                // Tile is determined
                //GenerateTile(potentialTiles[0], tileGroupsByUID[potentialTiles[0].groupUID], index);
                mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
                mapRepresentation[index.Item1, index.Item2, index.Item3].Add(potentialTiles[0]);
                entropicMap.Remove(index);
            }
            else
            {
                // Tile entropy reduced
                entropicMap.Update(potentialTiles.Count, index);
            }

            // Add to the update queue
            if (!inQueue.Contains(index))
            {
                updateQueue.Enqueue(index);
                inQueue.Add(index);
            }
        }
    }

    private void GenerateTile(WFCTile tile, WFCTileGroup group, (int, int, int) index)
    {
        mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
        mapRepresentation[index.Item1, index.Item2, index.Item3].Add(tile);

        /*if (tile.content == null) return;
        GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        visuals.transform.position += transform.position + new Vector3(index.Item1, index.Item2, index.Item3)
            - group.bounds.center + group.bounds.extents;
        visuals.transform.parent = transform;*/
    }

    private void OnDrawGizmosSelected()
    {
        if (mapRepresentation == null || tileGroups == null) return;
        for (int x = 0; x < mapRepresentation.x; x++)
        {
            for (int y = 0; y < mapRepresentation.y; y++)
            {
                for (int z = 0; z < mapRepresentation.z; z++)
                {
                    if (updating.Item1 == x && updating.Item2 == y && updating.Item3 == z)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector3(x, y, z) + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
                    }
                    else
                    {
                        if (IsBufferTile((x,y,z))) continue;
                        if(mapRepresentation[x, y, z].Count <= 1)
                        {
                            Gizmos.color = Color.blue;
                        }
                        else
                        {
                            float a = (1f - mapRepresentation[x, y, z].Count * 1f / tileCount);
                            if (a == 0) continue;
                            Gizmos.color = new Color(a, a, a, a * 0.4f);
                        }

                            
                        Gizmos.DrawCube(new Vector3(x, y, z) + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
                    }
                }
            }
        }
    }
}

