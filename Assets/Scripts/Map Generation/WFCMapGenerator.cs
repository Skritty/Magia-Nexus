using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using TreeEditor;
using UnityEngine;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize;
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

    private List<WFCTile> GetMapTiles((int, int, int) index)
    {
        if ((uint)index.Item1 >= mapRepresentation.x || (uint)index.Item2 >= mapRepresentation.y || (uint)index.Item3 >= mapRepresentation.z) return null;
        return mapRepresentation[index.Item1, index.Item2, index.Item3];
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
        Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
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
                    }
                    else
                    {
                        tile.connections[i].allowedTiles.Add(adjecentTiles[i]);
                    }
                }
            }

            // Handle group links
            foreach (WFCTile tile in tileGroup.subtiles)
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
            }

            // Handle connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
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
                            if (!tile.connections[i].allowedTiles.Any(x => x.groupUID == tileActual.groupUID))
                                tile.connections[i].allowedTiles.Add(tileActual);
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (!otherConnection.allowedTiles.Any(x => x.groupUID == tile.groupUID))
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
                            if (!tile.connections[i].allowedTiles.Any(x => x.groupUID == tileActual.groupUID))
                                tile.connections[i].allowedTiles.Add(tileActual);
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (!otherConnection.isInternalConnection && !otherConnection.allowedTiles.Any(x => x.groupUID == tile.groupUID))
                                otherConnection.allowedTiles.Add(tile);
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
        

        // Add all valid tiles in each group to the list of possible tiles.
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (!tile.isHole)
                {
                    tiles.Add(tile);
                }
            }
        }

        // Setup the initial "waveform"
        mapRepresentation = new(xSize, ySize, zSize);
        entropicMap = new(tiles.Count);
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    mapRepresentation[x, y, z] = new List<WFCTile>();
                    mapRepresentation[x, y, z].AddRange(tiles);
                    entropicMap.Add(tiles.Count - 1, (x,y,z));
                }
            }
        }

        // Solve the WFC
        while (entropicMap.Count > 0)
        {
            (int, int, int) index = entropicMap.GetRandomAtLowestEntropy();
            WeightedChance<WFCTile> potentialTiles = new();
            foreach(WFCTile tile in mapRepresentation[index.Item1, index.Item2, index.Item3])
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();
            GenerateTile(selectedTile, index);

            while (updateQueue.Count > 0)
            {
                (int, int, int) updatePosition = updateQueue.Dequeue();
                inQueue.Remove(updatePosition);
                Observe(updatePosition.Item1, updatePosition.Item2, updatePosition.Item3);
                if (solveDelay > 0) yield return new WaitForSeconds(solveDelay);
            }
            if (creationDelay > 0) yield return new WaitForSeconds(creationDelay);
        }

        foreach(WFCTileGroup tileGroup in tileGroups)
        {
            Destroy(tileGroup.gameObject);
        }
        tileGroups.Clear();
    }

    private void Observe(int x, int y, int z)
    {
        List<WFCTile> tiles = mapRepresentation[x, y, z];
        Collapse(tiles, 0, (x + 1, y, z));
        Collapse(tiles, 1, (x - 1, y, z));
        Collapse(tiles, 2, (x, y + 1, z));
        Collapse(tiles, 3, (x, y - 1, z));
        Collapse(tiles, 4, (x, y, z + 1));
        Collapse(tiles, 5, (x, y, z - 1));
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
        // Does this tile have yet to be solved?
        if (!entropicMap.Contains(index)) return;

        // Is this connection a wildcard?
        List<WFCTile> allowedTiles = GetAllowedTiles(possibleConnections, connectionIndex);
        if (allowedTiles.Count == 0) return;

        updating = index;
        List<WFCTile> potentialTiles = GetMapTiles(index);
        int removed = potentialTiles.RemoveAll(x => !allowedTiles.Contains(x));
        if(removed > 0)
        {
            if(potentialTiles.Count == 0)
            {
                GenerateTile(ErrorTile.subtiles[0,0,0], index);
            }
            else if (potentialTiles.Count == 1)
            {
                GenerateTile(potentialTiles[0], index);
            }
            else if (!inQueue.Contains(index))
            {
                entropicMap.Update(potentialTiles.Count, index);
                updateQueue.Enqueue(index);
                inQueue.Add(index);
            }
        }
    }

    private void GenerateTile(WFCTile tile, (int, int, int) index)
    {
        mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
        mapRepresentation[index.Item1, index.Item2, index.Item3].Add(tile);
        entropicMap.Remove(index);
        if (!inQueue.Contains(index))
        {
            updateQueue.Enqueue(index);
            inQueue.Add(index);
        }
        
        if (tile.content == null) return;
        Instantiate(tile.content,
            transform.position + new Vector3(index.Item1, index.Item2, index.Item3) - tile.position,
            Quaternion.identity, transform);
    }

    private void OnDrawGizmosSelected()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    if (mapRepresentation == null || mapRepresentation.objects.Length == 0 || mapRepresentation[x, y, z] == null || tileGroups == null) return;
                    float c = 1 - mapRepresentation[x, y, z].Count * 1f / tileGroups.Count;
                    Gizmos.color = new Color(c,c,c, Mathf.Clamp01(c * 0.4f + .1f));
                    if (updating.Item1 == x && updating.Item2 == y && updating.Item3 == z) Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(x, y, z) + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
                }
            }
        }
    }
}

