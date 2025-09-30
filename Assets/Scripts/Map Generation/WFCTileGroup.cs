using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WFCTileGroup : MonoBehaviour
{
    public bool doSelection;
    public ThreeDimensionalSpatialRepresentation<WFCTile> subtiles;
    public Vector3 extents = Vector3.zero;
    [SerializeReference]
    public WFCTile selectedTile;
    [SerializeReference]
    public WFCGroupConnection selectedConnection;

    private void OnDrawGizmos()
    {
        if(gameObject.GetHashCode() > 0)
        {
            foreach (WFCTile tile in subtiles)
            {
                if (tile.reference == null) continue;
                tile.reference.tileIndex = subtiles.GetIndex(tile);
            }
            CreateSubtiles();
        }
        DrawTileSelectors();
        DrawConnectionSelectors();
    }

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
        if (extents == bounds.extents) return;

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
