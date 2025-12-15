using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[RequireComponent(typeof(BoxCollider))]
public class WFCTileGroup : MonoBehaviour
{
    public bool reset;
    public bool doSelection;
    public string groupUID;
    [HideInInspector]
    public ThreeDimensionalSpatialRepresentation<WFCTile> subtiles;
    public Vector3 extents = Vector3.zero;
    public WFCTile selectedTile;
    public WFCConnection selectedConnection;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (subtiles == null || subtiles.x == 0) return;
        if (!string.IsNullOrEmpty(selectedTile.groupUID))
        {
            subtiles[selectedTile.x, selectedTile.y, selectedTile.z] = selectedTile;
        }
    }

    [Button("Reset")]
    public void CreateSubtiles()
    {
        groupUID = PrefabStageUtility.GetPrefabStage(gameObject).assetPath;
        if (string.IsNullOrEmpty(groupUID))
        {
            Debug.LogWarning($"{gameObject.name} is not a prefab! Cannot generate");
            return;
        }

        Bounds bounds = new();
        bounds.center = transform.position;
        foreach (Renderer render in GetComponentsInChildren<Renderer>())
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

                    WFCTile subtile = new WFCTile(groupUID, x, y, z);
                    subtile.position = transform.InverseTransformPoint(bounds.center - bounds.extents) + new Vector3(x, y, z);
                    subtiles[x, y, z] = subtile;

                    subtile.connections[0] = new WFCConnection((uint)(x + 1) >= (int)(bounds.extents.x * 2));
                    subtile.connections[1] = new WFCConnection((uint)(x - 1) >= (int)(bounds.extents.x * 2));
                    subtile.connections[2] = new WFCConnection((uint)(y + 1) >= (int)(bounds.extents.y * 2));
                    subtile.connections[3] = new WFCConnection((uint)(y - 1) >= (int)(bounds.extents.y * 2));
                    subtile.connections[4] = new WFCConnection((uint)(z + 1) >= (int)(bounds.extents.z * 2));
                    subtile.connections[5] = new WFCConnection((uint)(z - 1) >= (int)(bounds.extents.z * 2));
                }
            }
        }
        doSelection = true;
    }

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
            if (tile.connections.Length < 6) continue;
            DrawConnection(tile.connections[0], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.right);
            DrawConnection(tile.connections[1], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.left);
            DrawConnection(tile.connections[2], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.up);
            DrawConnection(tile.connections[3], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.down);
            DrawConnection(tile.connections[4], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.forward);
            DrawConnection(tile.connections[5], transform.TransformPoint(tile.position) + Vector3.one * 0.5f + 0.5f * Vector3.back);
        }
    }

    private void DrawConnection(WFCConnection connection, Vector3 position)
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
#endif
}
