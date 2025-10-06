using Sirenix.Utilities.Editor;
using TreeEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WFCTileGroup))]
public class Editor_WFCTileGroupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        foreach (GameObject obj in Selection.objects)
        {
            WFCTileGroup group = obj.GetComponent<WFCTileGroup>();
            if (group == null) continue;
            if (group.reset)
            {
                group.reset = false;
                group.CreateSubtiles(obj);
            }
            if (group.save)
            {
                group.save = false;
                group.SolveConnections(obj);
            }
        }
    }

    private void OnSceneGUI()
    {
        foreach (GameObject obj in Selection.objects)
        {
            WFCTileGroup group = obj.GetComponent<WFCTileGroup>();
            if (group == null || !group.doSelection || !Event.current.OnMouseDown(0)) continue;

            ConnectionSelection(group);
            TileSelection(group);
        }
    }

    private void TileSelection(WFCTileGroup group)
    {
        BoxCollider collider = group.GetComponent<BoxCollider>();
        collider.size = Vector3.one;
        group.selectedTile = default(WFCTile);

        foreach (WFCTile tile in group.subtiles)
        {
            collider.center = tile.position + Vector3.one * 0.5f;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!PhysicsSceneExtensions.GetPhysicsScene(group.gameObject.scene).Raycast(ray.origin, ray.direction)) continue;
            group.selectedTile = tile;
            break;
        }
    }

    private void ConnectionSelection(WFCTileGroup group)
    {
        BoxCollider collider = group.GetComponent<BoxCollider>();
        collider.size = Vector3.one * 0.25f;
        group.selectedConnection = null;
        foreach (WFCTile tile in group.subtiles)
        {
            if (tile.groupConnections.Length < 6) continue;
            if (group.selectedConnection == null) CheckConnection(0, group, tile.groupConnections[0], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.right);
            if (group.selectedConnection == null) CheckConnection(1,group, tile.groupConnections[1], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.left);
            if (group.selectedConnection == null) CheckConnection(2, group, tile.groupConnections[2], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.up);
            if (group.selectedConnection == null) CheckConnection(3, group, tile.groupConnections[3], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.down);
            if (group.selectedConnection == null) CheckConnection(4, group, tile.groupConnections[4], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.forward);
            if (group.selectedConnection == null) CheckConnection(5, group, tile.groupConnections[5], collider, tile.position + Vector3.one * 0.5f + 0.5f * Vector3.back);
        }
    }

    private void CheckConnection(int i, WFCTileGroup group, WFCGroupConnection connection, BoxCollider collider, Vector3 center)
    {
        if (connection == null) return;
        collider.center = center;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!PhysicsSceneExtensions.GetPhysicsScene(group.gameObject.scene).Raycast(ray.origin, ray.direction)) return;
        group.selectedConnection = connection;
    }
}