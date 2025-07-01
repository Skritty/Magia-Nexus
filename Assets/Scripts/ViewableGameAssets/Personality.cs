using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/Personality")]
public class Personality : ViewableGameAsset
{
    [SerializeReference]
    public Targeting targeting;
}
