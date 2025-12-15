using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class WFCTile : IEquatable<WFCTile>
{
    public int x, y, z;
    public string groupUID;
    public GameObject content;

    [HideInInspector]
    public WFCConnection[] connections;
    public List<WFCTileRef> holeAllowedTileRefs;

    [HideInInspector]
    public Vector3 position;
    public bool IsHole => holeAllowedTileRefs.Count > 0;
    

    public WFCTile(string groupPrefabAssetPath, int x, int y, int z)
    {
        this.groupUID = groupPrefabAssetPath;
        this.x = x;
        this.y = y;
        this.z = z;
        content = null;
        holeAllowedTileRefs = new();
        connections = new WFCConnection[6];
        position = Vector3.zero;
    }

    public bool Equals(WFCTile other)
    {
        if (other != null && 
            groupUID == other.groupUID
            && x == other.x
            && y == other.y
            && z == other.z)
            return true;
        return false;
    }
}

[Serializable]
public class WFCConnection
{
    public List<WFCTileRef> allowedTileRefs = new();
    [HideInInspector]
    public List<WFCTile> allowedTiles = new();
    [HideInInspector]
    public bool displayConnectionGizmo;

    public WFCConnection(bool displayConnectionGizmo)
    {
        this.displayConnectionGizmo = displayConnectionGizmo;
    }
}

[Serializable]
public struct WFCTileRef
{
    public WFCTileGroup tileGroupPrefab;
    public string groupUID;
    public int x, y, z;

    public WFCTileRef(string groupUID, int x, int y, int z)
    {
        tileGroupPrefab = null;
        this.groupUID = groupUID;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // TODO: add better equality
}