using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WFCConnection
{
    [SerializeReference]
    public List<WFCTile> allowedTiles = new();
}
