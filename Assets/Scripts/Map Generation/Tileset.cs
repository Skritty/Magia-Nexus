using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Tileset")]
public class Tileset : ScriptableObject, ISerializationCallbackReceiver
{
    public static Tileset defaultTileset;

    public bool isDefaultTileset;
    public List<GenerationTile> tiles = new();

    public List<WFCTileGroup> tileGroups = new();

#if UNITY_EDITOR
    public void OnAfterDeserialize()
    {
        if (defaultTileset != null && defaultTileset != this) isDefaultTileset = false;
        else if (isDefaultTileset) defaultTileset = this;
    }

    public void OnBeforeSerialize() { }

    //[Button("Solve Connections")]
    //[InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed")]
    /*public void SolveConnections()
    {
        tiles.Clear();
        Dictionary<string, ThreeDimensionalSpatialRepresentation<WFCTile>> subtilesByGroupID = new();

        int tileCount = 0;
        foreach (WFCTileGroup groupPrefab in tileGroups)
        {
            subtilesByGroupID.Add(groupPrefab.groupUID, groupPrefab.subtiles.Copy());
            foreach (WFCTile tile in subtilesByGroupID[groupPrefab.groupUID])
            {
                if (tile.isHole) continue;
                tile.name = $"{groupPrefab.name} ({tile.index})";
                tiles.Add(tile);
                tileCount++;
            }
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].tileBit = new TileSuperposition(tileCount).SetBit(i, true);
        }

        return;
        // Set up connections with loaded tile groups
        foreach (KeyValuePair<string, ThreeDimensionalSpatialRepresentation<WFCTile>> subtileKVP in subtilesByGroupID)
        {
            // Handle internal connections
            foreach (WFCTile tile in subtileKVP.Value)
            {
                WFCTile[] adjecentTiles = subtileKVP.Value.GetAdjecentObjects(tile);
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
                        tile.connections[i].allowedTiles.Or(adjecentTiles[i].tileBit);
                        tile.connections[i].isInternalConnection = true;
                    }
                }
            }

            // Handle group links
            *//*foreach (WFCTile tile in tileGroup.subtiles)
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
            }*//*

            // Handle connections
            foreach (WFCTile tile in subtileKVP.Value)
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
                        *//*foreach (WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            otherConnection.allowedTiles |= tile.tileBit;
                        }*//*
                    }
                    else
                    {
                        foreach (WFCTileRef connectedTileRef in tile.connections[i].allowedTileRefs)
                        {
                            if (!subtilesByGroupID.ContainsKey(connectedTileRef.groupUID))
                            {
                                // Tile group referened doesn't exist in this generation
                                continue;
                            }
                            WFCTile tileActual = subtilesByGroupID[connectedTileRef.groupUID][connectedTileRef.x, connectedTileRef.y, connectedTileRef.z];
                            tile.connections[i].allowedTiles.Or(tileActual.tileBit);
                            tileActual.connections[reciprocatedIndex].allowedTiles.Or(tile.tileBit);
                        }
                    }
                }
            }
        }

        this.tiles = tiles;
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
                        Debug.LogWarning($"{group1.gameObject.name} overlaps with connection {group2.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }*/
#endif
}