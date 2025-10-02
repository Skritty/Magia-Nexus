using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WFCTileGroup : MonoBehaviour
{
    public bool reset, save;
    public bool doSelection;
    public ThreeDimensionalSpatialRepresentation<WFCTile> subtiles;
    public Vector3 extents = Vector3.zero;
    [SerializeReference]
    public WFCTile selectedTile;
    [SerializeReference]
    public WFCGroupConnection selectedConnection;

    [Button("Reset")]
    private void CreateSubtiles()
    {
        Bounds bounds = new();
        bounds.center = transform.position;
        foreach (Renderer render in gameObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(render.bounds);
        }
        //Gizmos.color = new Color(.8f, .8f, .8f, .1f);
        //Gizmos.DrawCube(bounds.center, bounds.size + new Vector3(.1f, .1f, .1f));
        //if (extents == bounds.extents) return;

        // Generate subtiles
        ThreeDimensionalSpatialRepresentation<WFCTile> oldSubtiles = null;
        oldSubtiles = subtiles;
        subtiles = new((int)(bounds.extents.x * 2), (int)(bounds.extents.y * 2), (int)(bounds.extents.z * 2));
        extents = bounds.extents;

        for (int x = 0; x < (int)(bounds.extents.x * 2); x++)
        {
            for (int y = 0; y < (int)(bounds.extents.y * 2); y++)
            {
                for (int z = 0; z < (int)(bounds.extents.z * 2); z++)
                {
                    /*try
                    {
                        if (oldSubtiles[x, y, z] != null)
                        {
                            subtiles[x, y, z] = oldSubtiles[x, y, z];
                            oldSubtiles[x, y, z] = null;
                            // TODO: delete no longer used connections
                            continue;
                        }
                    }
                    catch { }*/

                    WFCTile subtile = new WFCTile();
                    subtile.position = transform.InverseTransformPoint(bounds.center - bounds.extents) + new Vector3(x, y, z);
                    subtiles[x, y, z] = subtile;

                    subtile.groupConnections[0] = (uint)(x + 1) >= (int)(bounds.extents.x * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[1] = (uint)(x - 1) >= (int)(bounds.extents.x * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[2] = (uint)(y + 1) >= (int)(bounds.extents.y * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[3] = (uint)(y - 1) >= (int)(bounds.extents.y * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[4] = (uint)(z + 1) >= (int)(bounds.extents.z * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[5] = (uint)(z - 1) >= (int)(bounds.extents.z * 2) ? new WFCGroupConnection() : null;
                }
            }
        }
    }

    [Button("Save")]
    public void SolveConnections()
    {
        foreach (WFCTile tile in subtiles)
        {
            // Skip hole tiles (they are considered to not exist)
            if (tile.IsHole) continue;

            tile.tileGroup = this;
            // Link this tile to its tile reference
            if (tile.reference != null)
            {
                (int,int,int) index = subtiles.GetIndex(tile);
                tile.reference.xIndex = index.Item1;
                tile.reference.yIndex = index.Item2;
                tile.reference.zIndex = index.Item3;
            }

            // Set up internal and external connections to actual tile references
            WFCTile[] adjecentTiles = subtiles.GetAdjecentObjects(tile);
            for (int i = 0; i < 6; i++)
            {
                tile.connections[i] = new WFCConnection();

                // Extract tile references
                if (tile.groupConnections[i] != null)
                {
                    tile.connections[i].allowedTiles.AddRange(ExtractTileReferences(tile.groupConnections[i].allowedTiles));
                }

                if (adjecentTiles[i] == null) continue;
                if (adjecentTiles[i].IsHole)
                {
                    // Extract the hole's tile references
                    tile.connections[i].allowedTiles.AddRange(ExtractTileReferences(adjecentTiles[i].allowedTiles));
                }
                else
                {
                    // Connect the adjecent internal tile
                    Debug.Log(adjecentTiles[i].GetHashCode());
                    tile.connections[i].allowedTiles.Add(adjecentTiles[i]);
                }
            }
        }
        foreach (WFCTile tile in subtiles)
        {
            // Ensure that connections link properly with other tiles in each group
            for (int i = 0; i < 6; i++)
            {
                foreach (WFCTile connectedTile in tile.connections[i].allowedTiles)
                {
                    // Ignore internal tiles
                    if (connectedTile.tileGroup == this) continue;

                    // Find the index where the connecting tile would be
                    (int, int, int) selfIndex = subtiles.GetIndex(tile);
                    selfIndex = (selfIndex.Item1 + (i == 0 || i == 1 ? -(i % 2 * 2 - 1) : 0),
                        selfIndex.Item2 + (i == 2 || i == 3 ? -(i % 2 * 2 - 1) : 0),
                        selfIndex.Item3 + (i == 4 || i == 5 ? -(i % 2 * 2 - 1) : 0));
                    ConnectOtherGroup(connectedTile.tileGroup, selfIndex, connectedTile.tileGroup.subtiles.GetIndex(connectedTile));
                }
            }
        }
    }

    private List<WFCTile> ExtractTileReferences(List<WFCTileReferenceSO> tileRefs)
    {
        // TODO: replace tile refs and "connections" with a tool that lets you move the offset of a group you want at a spot
        List<WFCTile> tiles = new();
        foreach(WFCTileReferenceSO tileRef in tileRefs)
        {
            if(tileRef.group == null)
            {
                Debug.LogWarning($"{tileRef.name} has not been set");
            }
            WFCTile tile = tileRef.group.subtiles[tileRef.xIndex, tileRef.yIndex, tileRef.zIndex];
            if(tile.tileGroup == null)
            {
                Debug.LogWarning($"{tileRef.name} has not been correctly linked, try saving the group");
            }
            tiles.Add(tile);
            Debug.Log("Object: " + GetHashCode());
            Debug.Log("Subtiles: " + subtiles.GetHashCode());
            Debug.Log("Tile: " + subtiles[tileRef.xIndex, tileRef.yIndex, tileRef.zIndex].GetHashCode());
            Debug.Log("Prefab: " + tileRef.group.GetHashCode());
            Debug.Log("Prefab subtiles: " + tileRef.group.subtiles.GetHashCode());
            Debug.Log("Prefab Tile: " + tile.GetHashCode());
        }
        return tiles;
    }

    private void ConnectOtherGroup(WFCTileGroup group, (int, int, int) initialSelfIndex, (int, int, int) initialConnectionIndex)
    {
        // Iterate through self group subtiles (plus 1 around)
        for (int x = -1; x <= subtiles.x; x++)
        {
            for (int y = -1; y <= subtiles.y; y++)
            {
                for (int z = -1; z <= subtiles.z; z++)
                {
                    (int, int, int) connectionIndex = (
                        initialConnectionIndex.Item1 - initialSelfIndex.Item1 + x,
                        initialConnectionIndex.Item2 - initialSelfIndex.Item2 + y,
                        initialConnectionIndex.Item3 - initialSelfIndex.Item3 + z);

                    if ((uint)connectionIndex.Item1 >= group.subtiles.x
                        || (uint)connectionIndex.Item2 >= group.subtiles.y
                        || (uint)connectionIndex.Item3 >= group.subtiles.z)
                        continue;

                    WFCTile connectionTile = group.subtiles[connectionIndex];
                    WFCTile selfTile = null;
                    if ((uint)x < subtiles.x && (uint)y < subtiles.y && (uint)z < subtiles.z)
                    {
                        selfTile = subtiles[x, y, z];
                    }

                    if (selfTile == null || selfTile.IsHole)
                    {
                        WFCTile[] holeAdjacentTiles = subtiles.GetAdjecentObjects((x, y, z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i] == null || holeAdjacentTiles[i].IsHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            if (connection == null) connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)] = new WFCConnection();
                            else if (connection.allowedTiles.Contains(connectionTile)) continue;
                            connection.allowedTiles.Add(connectionTile);
                            Debug.Log($"Connecting {group.gameObject.name} ({initialSelfIndex}) to {gameObject.name} ({initialConnectionIndex}) at connection index {i + (i % 2 > 0 ? -1 : 1)} | ");
                            // xyz = (0,0,-1), initialSelfIndex = (0,0,-1)
                            // initialConnectionIndex = (0,0,0), goalIndex = (1,0,0)
                            // initialConnectionIndex - initialSelfIndex + xyz = goalIndex
                        }
                    }
                    else
                    {
                        // Overlaps!
                        Debug.LogWarning($"{gameObject.name} overlaps with connection {group.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (reset)
        {
            reset = false;
            CreateSubtiles();
        }
        if (save)
        {
            save = false;
            SolveConnections();
        }
        DrawTileSelectors();
        DrawConnectionSelectors();
    }

    private void DrawTileSelectors()
    {
        foreach (WFCTile tile in subtiles)
        {
            if(tile == selectedTile)
            {
                Gizmos.color = new Color(.8f, .3f, .1f, .6f);
            }
            else
            {
                Gizmos.color = new Color(.3f, .3f, .3f, .1f);
            }
            Gizmos.DrawCube(transform.TransformPoint(tile.position) + Vector3.one * 0.5f, Vector3.one * 1.05f);
        }
    }

    private void DrawConnectionSelectors()
    {
        foreach (WFCTile tile in subtiles)
        {
            DrawConnection(tile.groupConnections[0], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.right);
            DrawConnection(tile.groupConnections[1], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.left);
            DrawConnection(tile.groupConnections[2], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.up);
            DrawConnection(tile.groupConnections[3], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.down);
            DrawConnection(tile.groupConnections[4], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.forward);
            DrawConnection(tile.groupConnections[5], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.back);
        }
    }

    private void DrawConnection(WFCGroupConnection connection, Vector3 position)
    {
        if (connection == null) return;
        if (connection == selectedConnection)
        {
            Gizmos.color = new Color(1f, 0f, 0f, .8f);
        }
        else
        {
            Gizmos.color = new Color(.1f, .1f, .1f, .8f);
        }
        Gizmos.DrawCube(position, Vector3.one * .25f);
    }
}
