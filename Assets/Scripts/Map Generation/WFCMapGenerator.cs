using System.Collections.Generic;
using System.Linq;
using OpenCover.Framework.Model;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WFCMapGenerator : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public List<WFCTileGroup> tileGroupPrefabs;
    public ThreeDimensionalSpatialRepresentation<List<WFCTile>> mapRepresentation;
    private List<(int, int, int)> validIndicies = new();

    private void Start()
    {
        GenerateMap(xSize, ySize, zSize);
    }

    

    public void GenerateMap(int xSize, int ySize, int zSize)
    {
        List<WFCTile> tiles = new();
        Dictionary<string, WFCTileGroup> loadedTileGroups = new();
        
        foreach (WFCTileGroup groupPrefab in tileGroupPrefabs)
        {
            string prefabPath = AssetDatabase.GetAssetPath(groupPrefab.gameObject);
            WFCTileGroup tileGroup = new EditPrefabAssetScope(prefabPath).prefabRoot.GetComponent<WFCTileGroup>();
            loadedTileGroups.Add(prefabPath, tileGroup);
        }

        // Reciprocate connections to loaded tile groups (these will not be saved to prefabs)
        foreach (KeyValuePair<string, WFCTileGroup> loadedTileGroup in loadedTileGroups)
        {
            foreach (WFCTile tile in loadedTileGroup.Value.subtiles)
            {
                if (tile.IsHole) continue;
                // Reciprocate connections
                for(int i = 0; i < 6; i++)
                {
                    foreach(WFCTile connectedTile in tile.connections[i].allowedTiles)
                    {
                        if (tile.groupPrefabAssetPath == connectedTile.groupPrefabAssetPath || !loadedTileGroups.ContainsKey(connectedTile.groupPrefabAssetPath)) continue;
                        WFCTile tileActual = loadedTileGroups[connectedTile.groupPrefabAssetPath].subtiles[connectedTile.xIndex, connectedTile.yIndex, connectedTile.zIndex];
                        List<WFCTile> otherAllowed = tileActual.connections[i + (i % 2 > 0 ? -1 : 1)].allowedTiles;
                        if (!otherAllowed.Contains(tile))
                        {
                            otherAllowed.Add(tile);
                        }
                    }
                    
                }
            }
        }

        // Add all valid tiles in each group to the list of possible tiles. TODO: perhaps add weighting
        foreach (KeyValuePair<string, WFCTileGroup> loadedTileGroup in loadedTileGroups)
        {
            foreach (WFCTile tile in loadedTileGroup.Value.subtiles)
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
                    Instantiate(mapRepresentation[x, y, z][0].content, transform.position + new Vector3(x,y,z) - mapRepresentation[x, y, z][0].position, Quaternion.identity, transform);
                }
            }
        }

        foreach (KeyValuePair<string, WFCTileGroup> group in loadedTileGroups)
        {
            PrefabUtility.UnloadPrefabContents(group.Value.gameObject);
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

    private List<WFCTile> CompileAllowedTiles(List<WFCTile> tiles, int connectionIndex)
    {
        List<WFCTile> allowed = new();
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

    private void Collapse(List<WFCTile> allowedTiles, int x, int y, int z)
    {
        if (allowedTiles.Count == 0
            || (uint)x >= mapRepresentation.x
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

