using System;
using UnityEngine;

[Serializable]
public class WFCTile : WFCGroupConnection
{
    public WFCTileReferenceSO reference;
    public Vector3 position;
    public WFCTileGroup tileGroup;
    public GameObject content;
    public WFCConnection[] connections = new WFCConnection[6];
    public WFCGroupConnection[] groupConnections = new WFCGroupConnection[6];
    public bool IsHole => allowedTiles.Count > 0;
}