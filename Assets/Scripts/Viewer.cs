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
    public CharacterAI autoplayAI;
    [SerializeReference]
    public Personality personality;
    public List<Item> items = new List<Item>();
    public List<Action> actions = new List<Action>();
    public Entity character;

    // Persistent Data
    public bool autoplay = true;
    public float points;
    public float roundPoints;
    public float killGainMultiplier;
    public int gold;
    public int totalGold;
    public int deaths;
    public int wins;
    public int losses;
    public int winstreak;
    public bool lockTargeting;
    public Dictionary<Viewer, float> killedBy = new Dictionary<Viewer, float>();
    public HashSet<string> unlockedClasses = new HashSet<string>();
}