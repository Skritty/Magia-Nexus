using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ViewableGameAsset/Class")]
public class CharacterClass : ViewableGameAsset
{
    public Personality defaultPersonality;
    public CharacterAI autoplayAI;
    public List<Item> items;
}
