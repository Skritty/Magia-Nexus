using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public List<WFCTileGroup> tileGroupPrefabs;
    public ThreeDimensionalSpatialRepresentation<List<WFCTile>> mapRepresentation;
    public float creationDelayBranch, creationDelayTile;
    public GameObject ErrorTile;

    private List<(int, int, int)> validIndicies = new();
    private List<(int, int, int)> determinedTiles = new();

    [SerializeField]
    private List<WFCTileGroup> tileGroups = new();

    private void Start()
    {
        SolveConnections();
        GenerateMap(xSize, ySize, zSize);
        //StartCoroutine(GenerateMap(xSize, ySize, zSize));
    }

    [InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed"), Button("Solve Connections")]
    public void SolveConnections()
    {
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            if (tileGroup == null) continue;
            DestroyImmediate(tileGroup.gameObject);
        }
        tileGroups.Clear();
        Dictionary<string, WFCTileGroup> tileGroupsByUID = new();

        // Create prefab instances, put them into UID dictionary
        foreach (WFCTileGroup groupPrefab in tileGroupPrefabs)
        {
            WFCTileGroup loadedTileGroup = Instantiate(groupPrefab);
            loadedTileGroup.gameObject.SetActive(false);
            loadedTileGroup.transform.parent = transform;
            tileGroups.Add(loadedTileGroup);
            tileGroupsByUID.Add(loadedTileGroup.groupUID, loadedTileGroup);
        }

        // Set up connections with loaded tile groups
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            // Handle internal connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                WFCTile[] adjecentTiles = tileGroup.subtiles.GetAdjecentObjects(tile);
                for (int i = 0; i < 6; i++)
                {
                    if (adjecentTiles[i] == null) continue;
                    if (adjecentTiles[i].IsHole)
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
                        WFCConnection otherConnection = tileActual.connections[i + (i % 2 > 0 ? -1 : 1)];
                        if (otherConnection.allowedTileRefs.Count != 0 && !otherConnection.allowedTiles.Any(x => x.groupUID == tile.groupUID))
                            otherConnection.allowedTiles.Add(tile);
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

                    if(group2.subtiles[connectionIndex].IsHole) continue;

                    WFCTile selfTile = null;
                    if ((uint)x < group1.subtiles.x && (uint)y < group1.subtiles.y && (uint)z < group1.subtiles.z)
                    {
                        selfTile = group1.subtiles[x, y, z];
                    }

                    if (selfTile == null || selfTile.IsHole)
                    {
                        WFCTile[] holeAdjacentTiles = group1.subtiles.GetAdjecentObjects((x, y, z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i] == null || holeAdjacentTiles[i].IsHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            WFCTileRef tileRef = new WFCTileRef(group2.groupUID, connectionIndex.Item1, connectionIndex.Item2, connectionIndex.Item3);
                            if (!connection.allowedTileRefs.Contains(tileRef)) connection.allowedTileRefs.Add(tileRef);

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

        // Add all valid tiles in each group to the list of possible tiles. TODO: perhaps add weighting
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (!tile.IsHole)
                {
                    tiles.Add(tile);
                }
            }
        }

        // Setup the initial "waveform"
        mapRepresentation = new(xSize, ySize, zSize);
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    mapRepresentation[x, y, z] = new List<WFCTile>();
                    mapRepresentation[x, y, z].AddRange(tiles);
                    validIndicies.Add((x, y, z));
                }
            }
        }

        // Solve the WFC
        int uhoh = 0;
        while (validIndicies.Count > 0)
        {
            uhoh++;
            int lowestChaos = int.MaxValue;
            foreach ((int, int, int) i in validIndicies)
            {
                if (mapRepresentation[i.Item1, i.Item2, i.Item3].Count < lowestChaos)
                {
                    lowestChaos = mapRepresentation[i.Item1, i.Item2, i.Item3].Count;
                }
            }
            List<(int, int, int)> validIndiciesLowest = new();
            foreach ((int, int, int) i in validIndicies)
            {
                if(mapRepresentation[i.Item1, i.Item2, i.Item3].Count == lowestChaos)
                {
                    validIndiciesLowest.Add(i);
                }
            }
            (int, int, int) index = validIndiciesLowest[Random.Range(0, validIndiciesLowest.Count)];
            List<WFCTile> potentialTiles = mapRepresentation[index.Item1, index.Item2, index.Item3];
            WFCTile selectedTile;
            if (potentialTiles.Count == 0)
            {
                selectedTile = null;
            }
            else
            {
                WFCTile[] randomTileOptions = potentialTiles.Where(x => x.excludeSeedPick).ToArray();
                selectedTile = randomTileOptions[Random.Range(0, randomTileOptions.Length)];
            }
            mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
            mapRepresentation[index.Item1, index.Item2, index.Item3].Add(selectedTile);
            validIndicies.Remove(index);
            determinedTiles.Add(index);
            Observe(index.Item1, index.Item2, index.Item3);

            foreach((int, int, int) determinedIndex in determinedTiles)
            {
                if (mapRepresentation[determinedIndex.Item1, determinedIndex.Item2, determinedIndex.Item3].Count == 0) continue;
                WFCTile tile = mapRepresentation[determinedIndex.Item1, determinedIndex.Item2, determinedIndex.Item3][0];
                if (tile.content == null) continue;
                Instantiate(tile.content, 
                    transform.position + new Vector3(determinedIndex.Item1, determinedIndex.Item2, determinedIndex.Item3) 
                    - tile.position, 
                    Quaternion.identity, transform);
                //yield return new WaitForSeconds(creationDelayTile);
            }
            //if(determinedTiles.Count > 0) yield return new WaitForSeconds(creationDelayBranch);
            determinedTiles.Clear();
        }

        // Generate the map objects
        /*for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    if (mapRepresentation[x,y,z].Count == 0 || mapRepresentation[x, y, z][0].content == null) continue;
                    Instantiate(mapRepresentation[x, y, z][0].content, transform.position + new Vector3(x,y,z) - mapRepresentation[x, y, z][0].position, Quaternion.identity, transform);
                }
            }
        }*/
    }

    private void Observe(int x, int y, int z)
    {
        List<WFCTile> tiles = mapRepresentation[x, y, z];
        Collapse(GetAllowedTiles(tiles, 0), x + 1, y, z);
        Collapse(GetAllowedTiles(tiles, 1), x - 1, y, z);
        Collapse(GetAllowedTiles(tiles, 2), x, y + 1, z);
        Collapse(GetAllowedTiles(tiles, 3), x, y - 1, z);
        Collapse(GetAllowedTiles(tiles, 4), x, y, z + 1);
        Collapse(GetAllowedTiles(tiles, 5), x, y, z - 1);
    }

    private List<WFCTile> GetAllowedTiles(List<WFCTile> tiles, int connectionIndex)
    {
        List<WFCTile> allowed = new();
        foreach (WFCTile tile in tiles)
        {
            allowed.AddRange(tile.connections[connectionIndex].allowedTiles);
        }
        return allowed;
    }

    private void Collapse(List<WFCTile> allowedTiles, int x, int y, int z)
    {
        if (allowedTiles.Count == 0// || !validIndicies.Contains((x, y, z))
            || (uint)x >= mapRepresentation.x
            || (uint)y >= mapRepresentation.y
            || (uint)z >= mapRepresentation.z)
            return;

        HashSet<WFCTile> toRemove = new();
        List<WFCTile> potentialTiles = mapRepresentation[x, y, z];
        for (int i = 0; i < potentialTiles.Count; i++)
        {
            if (!allowedTiles.Contains(potentialTiles[i]))
                toRemove.Add(potentialTiles[i]);
        }
        if (toRemove.Count == 0) return;
        foreach (WFCTile tile in toRemove)
        {
            potentialTiles.Remove(tile);
        }
        if (potentialTiles.Count == 0)
        {
            string remove = "";
            foreach(WFCTile tr in toRemove)
            {
                remove += $"{tr.content.name}, ";
            }
            Debug.LogWarning($"Leaving a tile with no options! Removed: {remove}");
            Instantiate(ErrorTile,
                transform.position + new Vector3(x, y, z) + Vector3.one*0.5f,
                    Quaternion.identity, transform);
        }
        if (potentialTiles.Count == 1)
        {
            validIndicies.Remove((x, y, z));
            determinedTiles.Add((x, y, z));
        }
        Observe(x, y, z);
    }
}

