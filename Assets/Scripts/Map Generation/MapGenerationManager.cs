using System;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using UnityEngine;

public abstract class MapGenerator
{
    public abstract void Initialize(MapGenerationManager manager);
    public abstract void Generate(NTree<FlagList> spatialTree);
}
public class FlagList
{
    static readonly ulong ULOne = 1;
    public ulong options;
    public int Entropy
    {
        get
        {
            if (options == 0) return 0;
            int entropy = 0;
            for (int i = 0; i < 64; i++)
            {
                if ((options & (ULOne << i)) != 0) entropy++;
            }
            return entropy;
        }
    }

    public bool Solved => (options & (options - 1)) == 0;

    public FlagList(ulong options)
    {
        this.options = options;
    }

    public List<T> GetObjects<T>(params T[] objects)
    {
        List<T> objectsList = new();
        for (int i = 0; i < objects.Count(); i++)
        {
            if ((options & (ULOne << i)) != 0)
            {
                objectsList.Add(objects[i]);
            }
        }
        return objectsList;
    }
}
public class MapGenerationManager : Singleton<MapGenerationManager>
{
    //[NonSerialized]
    private NTree<FlagList>[] mapRepresentationLODs;
    public List<WFCTileGroup> tileGroups;
    [SerializeReference]
    public List<MapGenerator>[] generationLayers;

    private Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
    private int tileCount;
    private const ulong ULOne = 1;

    private void Start()
    {
        for (int i = 0; i < generationLayers.Length; i++)
        {
            if (generationLayers[i] == null) continue;
            mapRepresentationLODs[i] = new NTree<FlagList>();
            foreach (MapGenerator generator in generationLayers[i])
            {
                generator.Initialize(this);
            }
            foreach (MapGenerator generator in generationLayers[i])
            {
                generator.Generate(mapRepresentationLODs[0]);
            }
        }
    }
    
    public void GetFlagsAtLOD(int level, MultidimensionalPosition position)
    {
        position = position.Clone();
        position.MaskDimensions(~0 << level);
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
        tileGroups.Clear();
        tileGroupsByUID = new();
        List<WFCTile> allTiles = new List<WFCTile>();

        // Create prefab instances, put them into UID dictionary
        foreach (WFCTileGroup groupPrefab in tileGroups)
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
                        foreach (WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            otherConnection.allowedTiles |= tile.tileBit;
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
                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            tileActual.connections[reciprocatedIndex].allowedTiles |= tile.tileBit;
                        }
                    }
                }
            }
        }
    }
}
