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

    

    public void GenerateMap(int xSize, int ySize, int zSize)
    {
        List<WFCTile> tiles = new();

        // Add all valid tiles in each group to the list of possible tiles. TODO: perhaps add weighting
        foreach (WFCTileGroup group in referenceTileGroups)
        {
            foreach (WFCTile tile in group.subtiles)
            {
                if (!tile.IsHole)
                {
                    tiles.Add(tile);
                    Debug.Log(tile.GetHashCode());
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
            if(tile.connections[connectionIndex].Equals(default(WFCConnection)))
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

