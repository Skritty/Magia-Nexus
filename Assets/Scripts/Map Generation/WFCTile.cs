using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class WFCTile : GenerationTile, IEquatable<WFCTile>
{
    public string groupUID;
    public float weight;

    //[HideInInspector]
    public WFCConnection[] connections;
    public bool isHole;
    public List<WFCTileRef> holeAllowedTileRefs;

    //[HideInInspector]
    public Vector3 positionActual;
    [HideInInspector]
    public ulong tileBit;



    public WFCTile(string groupPrefabAssetPath, int x, int y, int z)
    {
        this.groupUID = groupPrefabAssetPath;
        position = new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z);
        content = null;
        holeAllowedTileRefs = new();
        connections = new WFCConnection[6];
        positionActual = Vector3.zero;
    }

    public bool Equals(WFCTile other)
    {
        if (other != null && 
            groupUID == other.groupUID
            && position.Equals(other.position))
            return true;
        return false;
    }
}

[Serializable]
public class WFCConnection
{
    public List<WFCTileRef> allowedTileRefs;
    [HideInInspector]
    public ulong allowedTiles;
    [HideInInspector]
    public bool isInternalConnection;

    public WFCConnection(bool displayConnectionGizmo)
    {
        allowedTileRefs = new();
        this.isInternalConnection = displayConnectionGizmo;
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