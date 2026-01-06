using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using UnityEngine;
using UnityEngine.Profiling;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize, bufferSize;
    public List<WFCTileGroup> tileGroupPrefabs;
    [NonSerialized]
    public ThreeDimensionalSpatialRepresentation<HashSet<WFCTile>> mapRepresentation;
    public float creationDelay, solveDelay;
    public WFCTileGroup ErrorTile;

    private EntropicSet<(int x, int y, int z)> entropicMap;
    private Queue<(int x, int y, int z)> updateQueue = new();
    private HashSet<(int x, int y, int z)> inQueue = new();
    

    [SerializeField]
    private List<WFCTileGroup> tileGroups = new();
    private Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
    private WeightedChance<WFCTile> potentialTiles = new();
    private int tileCount;
    private ulong allowed;
    private (int x, int y, int z) index;
    private (int x, int y, int z) updating;
    private const ulong ULOne = 1;

    private HashSet<WFCTile> GetMapTiles((int x, int y, int z) index)
    {
        if ((uint)index.x >= mapRepresentation.x || (uint)index.y >= mapRepresentation.y || (uint)index.z >= mapRepresentation.z) return null;
        return mapRepresentation[index.x, index.y, index.z];
    }

    private void Start()
    {
        SolveConnections();
        //GenerateMap(xSize, ySize, zSize);
        StartCoroutine(GenerateMap(xSize, ySize, zSize));
    }

    //[InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed"), Button("Solve Connections")]
    public void SolveConnections()
    {
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
                if (tile.isHole) continue;
                allTiles.Add(tile);
                tile.tileIndex = tileCount;
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
                        tile.connections[i].allowedTiles |= ULOne << adjecentTiles[i].tileIndex;
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
                        foreach (WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            tile.connections[i].allowedTiles |= ULOne << tileActual.tileIndex;
                            otherConnection.allowedTiles |= ULOne << tile.tileIndex;
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
                            tile.connections[i].allowedTiles |= ULOne << tileActual.tileIndex;
                            tileActual.connections[reciprocatedIndex].allowedTiles |= ULOne << tile.tileIndex;
                        }
                    }
                }
            }
        }
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

    public IEnumerator GenerateMap(int xSize, int ySize, int zSize)
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
                    mapRepresentation[x, y, z] = new HashSet<WFCTile>();
                    foreach(WFCTile tile in tiles)
                    {
                        mapRepresentation[x, y, z].Add(tile);
                    }
                    index = (x, y, z);
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
            index = entropicMap.GetRandomAtLowestEntropy();
            entropicMap.Remove(index);

            potentialTiles.Clear();
            foreach (WFCTile tile in mapRepresentation[index.x, index.y, index.z])
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();

            mapRepresentation[index.x, index.y, index.z].Clear();
            mapRepresentation[index.x, index.y, index.z].Add(selectedTile);
            updateQueue.Enqueue(index);
            inQueue.Add(index);
            GenerateTile(selectedTile, tileGroupsByUID[selectedTile.groupUID], index);
            while (updateQueue.Count > 0)
            {
                index = updateQueue.Dequeue();
                inQueue.Remove(index);
                Observe(index.x, index.y, index.z);
                if (solveDelay > 0) yield return new WaitForSeconds(solveDelay);
            }
            if (creationDelay > 0) yield return new WaitForSeconds(creationDelay);
        }

        // Instantiate tile visuals
        /*for (int x = 0; x < mapRepresentation.x; x++)
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
        }*/

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
        HashSet<WFCTile> tiles = mapRepresentation[x, y, z];
        Collapse(tiles, 0, (x + 1, y, z));
        Collapse(tiles, 1, (x - 1, y, z));
        Collapse(tiles, 2, (x, y + 1, z));
        Collapse(tiles, 3, (x, y - 1, z));
        Collapse(tiles, 4, (x, y, z + 1));
        Collapse(tiles, 5, (x, y, z - 1));
    }

    private ulong GetAllowedTiles(HashSet<WFCTile> tiles, int connectionIndex)
    {
        allowed = 0;
        foreach (WFCTile tile in tiles)
        {
            allowed |= tile.connections[connectionIndex].allowedTiles;
        }
        return allowed;
    }

    private void Collapse(HashSet<WFCTile> possibleConnections, int connectionIndex, (int x, int y, int z) index)
    {
        HashSet<WFCTile> potentialTiles = GetMapTiles(index);
        // Is this tile outside of the map or is it already solved?
        if (potentialTiles == null || potentialTiles.Count <= 1) return;

        ulong allowedTiles = GetAllowedTiles(possibleConnections, connectionIndex);
        //updating = index;
        int count = potentialTiles.Count;
        potentialTiles.RemoveWhere(x => (allowedTiles & (ULOne << x.tileIndex)) == 0);

        if (count != potentialTiles.Count)
        {
            if (potentialTiles.Count == 0)
            {
                // Error! No tile options left to pick
                GenerateTile(ErrorTile.subtiles[0, 0, 0], ErrorTile, index);
                mapRepresentation[index.x, index.y, index.z].Clear();
                mapRepresentation[index.x, index.y, index.z].Add(ErrorTile.subtiles[0, 0, 0]);
                entropicMap.Remove(index);
                return;
            }

            if (potentialTiles.Count == 1)
            {
                // Tile is determined
                foreach(WFCTile tile in potentialTiles)
                    GenerateTile(tile, tileGroupsByUID[tile.groupUID], index);
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
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Add(tile);

        if (tile.content == null) return;
        GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        visuals.transform.position += transform.position + new Vector3(index.Item1, index.Item2, index.Item3)
            - group.bounds.center + group.bounds.extents;
        visuals.transform.parent = transform;
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
                            continue;
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

