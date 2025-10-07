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
                group.CreateSubtiles();
            }
            if (group.save)
            {
                group.save = false;
                group.SolveConnections();
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
            if (group.selectedConnection == null) CheckConnection(tile, 0, group, collider, 0.5f * Vector3.right);
            if (group.selectedConnection == null) CheckConnection(tile, 1, group, collider, 0.5f * Vector3.left);
            if (group.selectedConnection == null) CheckConnection(tile, 2, group, collider, 0.5f * Vector3.up);
            if (group.selectedConnection == null) CheckConnection(tile, 3, group, collider, 0.5f * Vector3.down);
            if (group.selectedConnection == null) CheckConnection(tile, 4, group, collider, 0.5f * Vector3.forward);
            if (group.selectedConnection == null) CheckConnection(tile, 5, group, collider, 0.5f * Vector3.back);
        }
    }

    private void CheckConnection(WFCTile tile, int i, WFCTileGroup group, BoxCollider collider, Vector3 offset)
    {
        collider.center = tile.position + Vector3.one * 0.5f + offset;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!PhysicsSceneExtensions.GetPhysicsScene(group.gameObject.scene).Raycast(ray.origin, ray.direction)) return;
        group.selectedConnection = tile.groupConnections[i];
        group.selectedRealConnections = tile.connections[i];
    }
}