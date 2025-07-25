using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ViewableGameAsset : ScriptableObject
{
    public List<string> aliases = new List<string>();
    public new string name => aliases.Count == 0 ? "" : aliases[0];
    [TextArea]
    public string info;
    public Sprite image;
    public bool hidden;
    public Color UIColor = Color.white;

    public bool NameMatch(string s)
    {
        s = s.ToLower().Replace(" ", "");
        foreach (string name in aliases)
        {
            if (name.ToLower().Replace(" ", "").Equals(s)) return true;
        }
        return false;
    }
}
