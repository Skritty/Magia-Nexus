using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public List<WFCTileGroup> referenceTileGroups;
    public ThreeDimensionalSpatialRepresentation<List<WFCTile>> mapRepresentation;
    private List<(int, int, int)> validIndicies = new();

    private void Start()
    {
        GenerateMap(xSize, ySize, zSize);
    }

    private void ConnectionLinking(WFCTileGroup selfGroup, WFCTileGroup connectionGroup, (int, int, int) initialSelfIndex, (int, int, int) initialConnectionIndex)
    {
        // Iterate through self group subtiles (plus 1 around)
        for (int x = -1; x <= selfGroup.subtiles.x; x++)
        {
            for (int y = -1; y <= selfGroup.subtiles.y; y++)
            {
                for (int z = -1; z <= selfGroup.subtiles.z; z++)
                {
                    (int, int, int) connectionIndex = (
                        initialConnectionIndex.Item1 - initialSelfIndex.Item1 + x,
                        initialConnectionIndex.Item2 - initialSelfIndex.Item2 + y,
                        initialConnectionIndex.Item3 - initialSelfIndex.Item3 + z);

                    if ((uint)connectionIndex.Item1 >= connectionGroup.subtiles.x
                        || (uint)connectionIndex.Item2 >= connectionGroup.subtiles.y
                        || (uint)connectionIndex.Item3 >= connectionGroup.subtiles.z)
                        continue;

                    WFCTile connectionTile = connectionGroup.subtiles[connectionIndex];
                    WFCTile selfTile = null;
                    if ((uint)x < selfGroup.subtiles.x && (uint)y < selfGroup.subtiles.y && (uint)z < selfGroup.subtiles.z)
                    {
                        selfTile = selfGroup.subtiles[x, y, z];
                    }

                    if (selfTile == null || selfTile.IsHole)
                    {
                        WFCTile[] holeAdjacentTiles = selfGroup.subtiles.GetAdjecentObjects((x,y,z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i] == null || holeAdjacentTiles[i].IsHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            if (connection == null) connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)] = new WFCConnection();
                            else if (connection.allowedTiles.Contains(connectionTile)) continue;
                            connection.allowedTiles.Add(connectionTile);
                            Debug.Log($"Connecting {connectionGroup.gameObject.name} ({initialSelfIndex}) to {selfGroup.gameObject.name} ({initialConnectionIndex}) at connection index {i + (i % 2 > 0 ? -1 : 1)} | ");
                            // xyz = (0,0,-1), initialSelfIndex = (0,0,-1)
                            // initialConnectionIndex = (0,0,0), goalIndex = (1,0,0)
                            // initialConnectionIndex - initialSelfIndex + xyz = goalIndex
                        }
                    }
                    else
                    {
                        // Overlaps!
                        Debug.LogWarning($"{selfGroup.gameObject.name} overlaps with connection {connectionGroup.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }

    public void GenerateMap(int xSize, int ySize, int zSize)
    {
        // Finish setting up tiles within groups, ensuring that they are valid
        List<WFCTile> tiles = new();
        foreach (WFCTileGroup group in referenceTileGroups)
        {
            foreach (WFCTile tile in group.subtiles)
            {
                // Ensure that connections link properly with other tiles in each group
                // TODO: Make this happen in the editor
                if(tile.allowedTiles.Count > 0)
                {
                    foreach (WFCTileReferenceSO connectedTileReference in tile.allowedTiles)
                    {
                        ConnectionLinking(group, connectedTileReference.group, group.subtiles.GetIndex(tile), connectedTileReference.tileIndex);
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    if (tile.groupConnections[i] == null) continue;
                    foreach (WFCTileReferenceSO connectedTileReference in tile.groupConnections[i].allowedTiles)
                    {
                        (int, int, int) selfIndex = group.subtiles.GetIndex(tile);
                        selfIndex = (selfIndex.Item1 + (i == 0 || i == 1 ? -(i % 2 * 2 - 1) : 0),
                            selfIndex.Item2 + (i == 2 || i == 3 ? -(i % 2 * 2 - 1) : 0),
                            selfIndex.Item3 + (i == 4 || i == 5 ? -(i % 2 * 2 - 1) : 0));
                        ConnectionLinking(group, connectedTileReference.group, selfIndex, connectedTileReference.tileIndex);
                    }
                }

                // Set up internal connections
                if (!tile.IsHole)
                {
                    tiles.Add(tile);
                    WFCTile[] adjecentTiles = group.subtiles.GetAdjecentObjects(tile);
                    for(int i = 0; i < 6; i++)
                    {
                        if (adjecentTiles[i] == null || adjecentTiles[i].allowedTiles.Count > 0) continue;
                        if(tile.connections[i] == null) tile.connections[i] = new WFCConnection();
                        tile.connections[i].allowedTiles.Add(adjecentTiles[i]);
                    }
                }
                else
                {
                    WFCTile[] adjecentTiles = group.subtiles.GetAdjecentObjects(tile);
                    for (int i = 0; i < 6; i++)
                    {
                        if (adjecentTiles[i] == null || adjecentTiles[i].allowedTiles.Count > 0) continue;
                        if (tile.connections[i + (i % 2 > 0 ? -1 : 1)] == null) tile.connections[i + (i % 2 > 0 ? -1 : 1)] = new WFCConnection();
                        tile.connections[i + (i % 2 > 0 ? -1 : 1)].allowedTiles.AddRange(tile.allowedTiles.Select(x => x.group.subtiles[x.tileIndex]));
                    }
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
                    mapRepresentation[x, y, z] = new List<WFCTile>(tiles);
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
            List<WFCTile> potentialTileGroups = mapRepresentation[index.Item1, index.Item2, index.Item3];
            WFCTile selectedTile = potentialTileGroups[Random.Range(0, potentialTileGroups.Count)];
            mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
            mapRepresentation[index.Item1, index.Item2, index.Item3].Add(selectedTile);
            validIndicies.Remove(index);
            Observe(index.Item1, index.Item2, index.Item3);
        }

        // Generate the map objects
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    if (mapRepresentation[x,y,z].Count == 0 || mapRepresentation[x, y, z][0].content == null) continue;
                    Instantiate(mapRepresentation[x, y, z][0].content, transform.position + new Vector3(x,y,z), Quaternion.identity, transform);
                }
            }
        }
    }

    private void Observe(int x, int y, int z)
    {
        List<WFCTile> tiles = mapRepresentation[x, y, z];
        Collapse(CompileAllowedTiles(tiles, 0), x + 1, y, z);
        Collapse(CompileAllowedTiles(tiles, 1), x - 1, y, z);
        Collapse(CompileAllowedTiles(tiles, 2), x, y + 1, z);
        Collapse(CompileAllowedTiles(tiles, 3), x, y - 1, z);
        Collapse(CompileAllowedTiles(tiles, 4), x, y, z + 1);
        Collapse(CompileAllowedTiles(tiles, 5), x, y, z - 1);
    }

    private HashSet<WFCTile> CompileAllowedTiles(List<WFCTile> tiles, int connectionIndex)
    {
        HashSet<WFCTile> allowed = new();
        foreach (WFCTile tile in tiles)
        {
            if(tile.connections[connectionIndex] == null)
            {
                allowed.Clear();
                break;
            }
            else
            {
                allowed.AddRange(tile.connections[connectionIndex].allowedTiles);
            }
        }
        return allowed;
    }

    private void Collapse(HashSet<WFCTile> allowedTiles, int x, int y, int z)
    {
        if ((uint)x >= mapRepresentation.x
            || (uint)y >= mapRepresentation.y
            || (uint)z >= mapRepresentation.z)
            return;

        HashSet<WFCTile> toRemove = new();
        List<WFCTile> potentialTileGroups = mapRepresentation[x, y, z];
        for (int i = 0; i < potentialTileGroups.Count; i++)
        {
            if (allowedTiles.Contains(potentialTileGroups[i])) continue;
            toRemove.Add(potentialTileGroups[i]);
        }
        if (potentialTileGroups.Count <= 1) validIndicies.Remove((x,y,z));
        if (toRemove.Count == 0) return;
        foreach (WFCTile tile in toRemove)
        {
            potentialTileGroups.Remove(tile);
        }
        Observe(x, y, z);
    }
}

