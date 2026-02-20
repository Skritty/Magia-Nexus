using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class MapGenerator
{
    public abstract HashSet<MultidimensionalPosition> Generate(NTree<TileSuperposition> spatialTree, Bounds generationBounds, Dictionary<string, WFCTileGroup> tilesByGroupUID);
}

public static class BitArrayExtensions
{
    public static bool IsZero(this BitArray bitArray)
    {
        for(int i = 0; i < bitArray.Length; i++)
        {
            if (bitArray[i]) return false;
        }
        return true;
    }
}

[Serializable]
public class TileSuperposition // TODO: Make this a struct. Figure out marshalling/Native for the bit array
{
    private static int tileCount = 10; // Remove this

    [SerializeField]
    private List<WFCTileGroup> _tiles;
    [Button("Set")]
    public void Set()
    {
        options = new BitArray(tileCount);
        for (int i = 0; i < tileCount; i++)
        {
            if (_tiles.Count == i) break;
            if (_tiles[i] != null) options[i] = true;
        }
    }
    public WFCTileGroup AddTile
    {
        get => _tiles.Count > 0 ? _tiles[_tiles.Count - 1] : null;
        set
        {
            _tiles.Add(value);
            options[value.index] = true;
        }
    }
    [ShowInInspector]
    public BitArray options = new(tileCount);
    [HideInInspector]
    public bool generated;

    public int Entropy
    {
        get
        {
            int entropy = 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i]) entropy++;
            }
            return entropy;
        }
    }

    public bool Solved
    {
        get
        {
            bool solved = false;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i])
                {
                    if (solved) return false;
                    solved = true;
                }
            }
            return solved;
        }
    }

    public TileSuperposition()
    {
        options = new(tileCount);
    }

    public TileSuperposition(params bool[] flags)
    {
        options = new BitArray(flags);
    }

    public List<T> GetObjects<T>(params T[] objects)
    {
        List<T> objectsList = new();
        for (int i = 0; i < options.Length; i++)
        {
            if (objects.Length == i) break;
            if (options[i])
            {
                objectsList.Add(objects[i]);
            }
        }
        return objectsList;
    }

    /// <summary>
    /// Returns true if this contains all the same set bits as other
    /// </summary>
    public bool Contains(TileSuperposition other)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (other.options.Length == i) break;
            if (other.options[i])
            {
                if (!options[i]) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if this contains all the same set bits as other
    /// </summary>
    public bool ContainsAll(TileSuperposition other)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (other.options.Length == i) break;
            if (other.options[i])
            {
                if (!options[i]) return false;
            }
        }
        return true;
    }

    // Override equals as well? Consider consiquences
    /*public override bool Equals(object obj)
    {
        if (options.Length != other.options.Length) return false;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != other.options[i]) return false;
        }
        return true;
    }*/
}
public class MapGenerationManager : Skritty.Tools.Utilities.Singleton<MapGenerationManager>
{
    [NonSerialized]
    private NTree<TileSuperposition>[] mapRepresentationLODs;

    public Transform center;
    public Vector3 size;
    public WFCTileGroup ErrorTile;
    public List<WFCTileGroup> tileGroups;
    public GenerationLayer[] generationLayers = new GenerationLayer[0];

    [Serializable]
    public class GenerationLayer
    {
        [SerializeReference]
        public List<MapGenerator> mapGenerators = new();
    }

    private Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
    private Dictionary<string, WFCTile> tilesGroupsByUID = new();
    private int tileCount;
    private const ulong ULOne = 1;
    private Vector3 previousCenter, previousSize;

    private void Start()
    {
        SolveConnections();
        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        mapRepresentationLODs = new NTree<TileSuperposition>[generationLayers.Length];
        for (int i = 0; i < generationLayers.Length; i++)
        {
            if (generationLayers[i] == null) continue;
            if (mapRepresentationLODs[i] == null)
            {
                mapRepresentationLODs[i] = new NTree<TileSuperposition>();
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        if (centerSnapped != previousCenter || size != previousSize)
        {
            previousSize = size;
            previousCenter = centerSnapped;
            Generate();
        }
    }

    private void Generate()
    {
        List<WFCTile> tileSet = new();
        // Add all valid tiles in each group to the list of possible tiles.
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (!tile.isHole)
                {
                    tileSet.Add(tile);
                }
            }
        }
        ulong defaultTileFlags = ulong.MaxValue >> Mathf.Clamp(64 - tileSet.Count, 0, 64);

        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        Bounds generationBounds = new Bounds(centerSnapped - transform.position, size);
        for (int i = 0; i < generationLayers.Length; i++)
        {
            if (generationLayers[i] == null) continue;

            /*for (int x = (int)-generationBounds.extents.x; x < generationBounds.extents.x; x++)
            {
                for (int y = (int)-generationBounds.extents.y; y < generationBounds.extents.y; y++)
                {
                    for (int z = (int)-generationBounds.extents.z; z < generationBounds.extents.z; z++)
                    {
                        MultidimensionalPosition position = new MultidimensionalPosition((ushort)(generationBounds.center.x + x), (ushort)(generationBounds.center.y + y), (ushort)(generationBounds.center.z + z));
                        mapRepresentationLODs[i].TryAddData(new FlagList(defaultTileFlags), position, out _);
                    }
                }
            }*/

            List<MultidimensionalPosition> generatedTiles = new();
            foreach (MapGenerator generator in generationLayers[i].mapGenerators)
            {
                generatedTiles.AddRange(generator.Generate(mapRepresentationLODs[i], generationBounds, tileGroupsByUID));
            }
            foreach (MultidimensionalPosition tilePosition in generatedTiles)
            {
                List<WFCTile> tile = mapRepresentationLODs[i][tilePosition].GetObjects(tileSet.ToArray());
                mapRepresentationLODs[i][tilePosition].generated = true;
                if (tile.Count == 0)
                {
                    // Error
                    //GenerateTile(ErrorTile.subtiles.objects[0,0,0], ErrorTile, tilePosition);
                }
                else if (tile.Count == 1)
                {
                    GenerateTile(tile[0], tileGroupsByUID[tile[0].groupUID], tilePosition);
                }
            }
        }
    }

    private bool GenerateTile(WFCTile tile, WFCTileGroup group, MultidimensionalPosition position)
    {
        if (tile.content == null) return false;
        GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        visuals.transform.position += transform.position + position.ToVector3
            - group.bounds.center + group.bounds.extents;
        visuals.transform.parent = transform;
        return true;
    }

    public void GetTilesAtLOD(byte level, MultidimensionalPosition position)
    {
        position = position.Clone();
        position.PositionAtDepth(level - 1);
        mapRepresentationLODs[level].GetDataAtPosition(position);
    }

    public void SaveMap() { }
    public void LoadMap() { }

    //[InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed"), Button("Solve Connections")]
    public void SolveConnections()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        tileGroupsByUID = new();
        List<WFCTile> allTiles = new List<WFCTile>();

        WFCTileGroup[] tileGroupPrefabs = tileGroups.ToArray();
        tileGroups.Clear();
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
                tilesGroupsByUID.Add(loadedTileGroup.groupUID, tile);
                tile.tileBit = ULOne << tileCount;
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
                        tile.connections[i].allowedTiles |= adjecentTiles[i].tileBit;
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
                        /*foreach (WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            otherConnection.allowedTiles |= tile.tileBit;
                        }*/
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
                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            tileActual.connections[reciprocatedIndex].allowedTiles |= tile.tileBit;
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

                    if (group2.subtiles[connectionIndex].isHole) continue;

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
}
