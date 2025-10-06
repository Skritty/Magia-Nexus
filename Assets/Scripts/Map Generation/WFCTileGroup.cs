using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WFCTileGroup : MonoBehaviour
{
    public bool reset, save;
    public bool doSelection;
    public ThreeDimensionalSpatialRepresentation<WFCTile> subtiles;
    public Vector3 extents = Vector3.zero;
    public WFCTile selectedTile;
    public WFCGroupConnection selectedConnection;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (subtiles.x == 0) return;
        if (!string.IsNullOrEmpty(selectedTile.groupPrefabAssetPath))
        {
            subtiles[selectedTile.xIndex, selectedTile.yIndex, selectedTile.zIndex] = selectedTile;
        }
    }

    [Button("Reset")]
    public void CreateSubtiles(GameObject prefab)
    {
        //string prefabPath = AssetDatabase.GetAssetPath(prefab);
        string prefabPath = PrefabStageUtility.GetPrefabStage(prefab).assetPath;
        EditPrefabAssetScope prefabAsset = new EditPrefabAssetScope(prefabPath);
        WFCTileGroup tileGroupPrefab = prefabAsset.prefabRoot.GetComponent<WFCTileGroup>();

        Bounds bounds = new();
        bounds.center = tileGroupPrefab.transform.position;
        foreach (Renderer render in tileGroupPrefab.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(render.bounds);
        }
        //Gizmos.color = new Color(.8f, .8f, .8f, .1f);
        //Gizmos.DrawCube(bounds.center, bounds.size + new Vector3(.1f, .1f, .1f));
        //if (extents == bounds.extents) return;

        // Generate subtiles
        ThreeDimensionalSpatialRepresentation<WFCTile> oldSubtiles = null;
        oldSubtiles = tileGroupPrefab.subtiles;
        tileGroupPrefab.subtiles = new((int)(bounds.extents.x * 2), (int)(bounds.extents.y * 2), (int)(bounds.extents.z * 2));
        tileGroupPrefab.extents = bounds.extents;

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

                    WFCTile subtile = new WFCTile(prefabPath, x, y, z);
                    subtile.position = tileGroupPrefab.transform.InverseTransformPoint(bounds.center - bounds.extents) + new Vector3(x, y, z);
                    tileGroupPrefab.subtiles[x, y, z] = subtile;

                    subtile.groupConnections[0] = (uint)(x + 1) >= (int)(bounds.extents.x * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[1] = (uint)(x - 1) >= (int)(bounds.extents.x * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[2] = (uint)(y + 1) >= (int)(bounds.extents.y * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[3] = (uint)(y - 1) >= (int)(bounds.extents.y * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[4] = (uint)(z + 1) >= (int)(bounds.extents.z * 2) ? new WFCGroupConnection() : null;
                    subtile.groupConnections[5] = (uint)(z - 1) >= (int)(bounds.extents.z * 2) ? new WFCGroupConnection() : null;
                }
            }
        }
        tileGroupPrefab.doSelection = true;
        prefabAsset.Dispose();
    }

    [Button("Save")]
    public void SolveConnections(GameObject prefab)
    {
        Dictionary<string, EditPrefabAssetScope> loadedPrefabs = new();

        string prefabPath = PrefabStageUtility.GetPrefabStage(prefab).assetPath;
        EditPrefabAssetScope prefabAsset = new EditPrefabAssetScope(prefabPath);
        loadedPrefabs.Add(prefabPath, prefabAsset);

        WFCTileGroup tileGroupPrefab = prefabAsset.prefabRoot.GetComponent<WFCTileGroup>();

        if (tileGroupPrefab == null) return;

        foreach (WFCTile tile in tileGroupPrefab.subtiles)
        {
            // Skip hole tiles (they are considered to not exist)
            if (tile.IsHole) continue;

            // Link this tile to its tile reference
            if (tile.reference != null)
            {
                tile.reference.tileRef = tile;
            }

            // Set up internal and external connections to actual tile references
            WFCTile[] adjecentTiles = tileGroupPrefab.subtiles.GetAdjecentObjects(tile);
            for (int i = 0; i < 6; i++)
            {
                // External ref connections
                AddTileRefs(tile.groupConnections[i]?.allowedTileSOs, tile, i, loadedPrefabs);

                if (adjecentTiles[i].Equals(default(WFCTile))) continue;
                if (adjecentTiles[i].IsHole)
                {
                    // Extract the hole's tile references
                    AddTileRefs(adjecentTiles[i].allowedTileSOs, tile, i, loadedPrefabs);
                }
                else
                {
                    // Connect the adjecent internal tile
                    (int, int, int) adjecentIndex = tileGroupPrefab.subtiles.GetIndex(adjecentTiles[i]);
                    tile.connections[i].allowedTiles.Add(new WFCTile(prefabPath, adjecentIndex.Item1, adjecentIndex.Item2, adjecentIndex.Item3));
                }
            }
        }
        foreach (WFCTile tile in tileGroupPrefab.subtiles)
        {
            // Ensure that connections link properly with other tiles in each group
            for (int i = 0; i < 6; i++)
            {
                foreach (WFCTile connectedTile in tile.connections[i].allowedTiles.ToArray())
                {
                    // Ignore internal tiles
                    if (connectedTile.groupPrefabAssetPath == prefabPath) continue;

                    // Find the index where the connecting tile would be
                    (int, int, int) selfIndex = tileGroupPrefab.subtiles.GetIndex(tile);
                    selfIndex = (selfIndex.Item1 + (i == 0 || i == 1 ? -(i % 2 * 2 - 1) : 0),
                        selfIndex.Item2 + (i == 2 || i == 3 ? -(i % 2 * 2 - 1) : 0),
                        selfIndex.Item3 + (i == 4 || i == 5 ? -(i % 2 * 2 - 1) : 0));
                    ConnectOtherGroup(tileGroupPrefab, loadedPrefabs[connectedTile.groupPrefabAssetPath].prefabRoot.GetComponent<WFCTileGroup>(), selfIndex, (connectedTile.xIndex, connectedTile.yIndex, connectedTile.zIndex));
                }
            }
        }
        
        foreach (KeyValuePair<string, EditPrefabAssetScope> prefabScope in loadedPrefabs)
        {
            prefabScope.Value.Dispose();
        }
    }

    private void AddTileRefs(List<WFCTileReferenceSO> tileRefList, WFCTile selfTile, int connectionIndex, Dictionary<string, EditPrefabAssetScope> loadedPrefabs)
    {
        if (tileRefList == null) return;
        // TODO: replace tile ref SOs and "connections" with a tool that lets you move the offset of a group you want at a spot
        foreach(WFCTileReferenceSO tileRefSO in tileRefList)
        {
            WFCTile tileRef = tileRefSO.tileRef;

            // Add the TileRef to our connections
            selfTile.connections[connectionIndex].allowedTiles.Add(tileRef);

            // Get/load the prefab we are connecting to
            EditPrefabAssetScope prefabAsset = null;
            if (loadedPrefabs.ContainsKey(tileRef.groupPrefabAssetPath))
            {
                prefabAsset = loadedPrefabs[tileRef.groupPrefabAssetPath];
            }
            else
            {
                prefabAsset = new EditPrefabAssetScope(tileRef.groupPrefabAssetPath);
                loadedPrefabs.Add(tileRef.groupPrefabAssetPath, prefabAsset);
            }
            WFCTileGroup tileGroupPrefab = prefabAsset.prefabRoot.GetComponent<WFCTileGroup>();

            // Reciprocate the connection
            tileGroupPrefab.subtiles[tileRef.xIndex, tileRef.yIndex, tileRef.zIndex].connections[connectionIndex + (connectionIndex % 2 > 0 ? -1 : 1)].allowedTiles.Add(selfTile);
        }
    }

    private void ConnectOtherGroup(WFCTileGroup tileGroupPrefab, WFCTileGroup group, (int, int, int) initialSelfIndex, (int, int, int) initialConnectionIndex)
    {
        // Iterate through self group subtiles (plus 1 around)
        for (int x = -1; x <= tileGroupPrefab.subtiles.x; x++)
        {
            for (int y = -1; y <= tileGroupPrefab.subtiles.y; y++)
            {
                for (int z = -1; z <= tileGroupPrefab.subtiles.z; z++)
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
                    WFCTile selfTile = default(WFCTile);
                    if ((uint)x < tileGroupPrefab.subtiles.x && (uint)y < tileGroupPrefab.subtiles.y && (uint)z < tileGroupPrefab.subtiles.z)
                    {
                        selfTile = tileGroupPrefab.subtiles[x, y, z];
                    }

                    if (selfTile.Equals(default(WFCTile)) || selfTile.IsHole)
                    {
                        WFCTile[] holeAdjacentTiles = tileGroupPrefab.subtiles.GetAdjecentObjects((x, y, z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i].Equals(default(WFCTile)) || holeAdjacentTiles[i].IsHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            if (connection.Equals(default(WFCConnection))) connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)] = new WFCConnection();
                            else if (connection.allowedTiles.Contains(connectionTile)) continue;
                            connection.allowedTiles.Add(connectionTile);

                            // Reciprocate the connection
                            if (!connectionTile.connections[i].allowedTiles.Contains(holeAdjacentTiles[i]))
                            {
                                connectionTile.connections[i].allowedTiles.Add(holeAdjacentTiles[i]);
                            }
                            Debug.Log($"Connecting {group.gameObject.name} ({initialSelfIndex}) to {tileGroupPrefab.gameObject.name} ({initialConnectionIndex}) at connection index {i + (i % 2 > 0 ? -1 : 1)} | ");
                            // xyz = (0,0,-1), initialSelfIndex = (0,0,-1)
                            // initialConnectionIndex = (0,0,0), goalIndex = (1,0,0)
                            // initialConnectionIndex - initialSelfIndex + xyz = goalIndex
                        }
                    }
                    else
                    {
                        // Overlaps!
                        Debug.LogWarning($"{tileGroupPrefab.gameObject.name} overlaps with connection {group.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }
#endif

    private void OnDrawGizmos()
    {
        DrawTileSelectors();
        DrawConnectionSelectors();
    }

    private void DrawTileSelectors()
    {
        if (subtiles.GetEnumerator() == null) return;
        foreach (WFCTile tile in subtiles)
        {
            if(tile.Equals(selectedTile))
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
        if (subtiles.GetEnumerator() == null) return;
        foreach (WFCTile tile in subtiles)
        {
            if (tile.groupConnections.Length < 6) continue;
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
