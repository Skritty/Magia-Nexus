using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Viewer
{
    public int viewerID;
    public string viewerName;
    public float points;
    public int currency;
    [SerializeReference]
    public Targeting targetType = new Targeting_Distance();
    public List<Item> items = new List<Item>();
    public List<Action> actions = new List<Action>();
}