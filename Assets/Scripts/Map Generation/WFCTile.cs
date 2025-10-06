using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WFCTile
{
    public int xIndex, yIndex, zIndex;
    public string groupPrefabAssetPath;
    public WFCTileReferenceSO reference;
    public GameObject content;

    public List<WFCTileReferenceSO> allowedTileSOs;
    public WFCConnection[] connections;
    public WFCGroupConnection[] groupConnections;

    [HideInInspector]
    public Vector3 position;
    public bool IsHole => allowedTileSOs.Count > 0;
    

    public WFCTile(string groupPrefabAssetPath, int xIndex, int yIndex, int zIndex)
    {
        this.groupPrefabAssetPath = groupPrefabAssetPath;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
        this.zIndex = zIndex;
        reference = null;
        content = null;
        allowedTileSOs = new();
        connections = new WFCConnection[6];
        groupConnections = new WFCGroupConnection[6];
        position = Vector3.zero;
    }
}

[Serializable]
public struct WFCConnection
{
    public List<WFCTile> allowedTiles;

    public WFCConnection(bool bleh = true)
    {
        allowedTiles = new();
    }
}