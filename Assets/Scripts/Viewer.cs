using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Viewer
{
    // Player Info
    public int viewerID;
    public string viewerName;
    
    // Character Info
    [SerializeReference]
    public Targeting targetType = new Targeting_Distance();
    public List<Item> items = new List<Item>();
    public List<Action> actions = new List<Action>();
    public Entity character;

    // Persistent Data
    public float points;
    public float roundPoints;
    public float killGainMultiplier;
    public int gold;
    public int totalGold;
    public Dictionary<Viewer, float> killedBy = new Dictionary<Viewer, float>();
}