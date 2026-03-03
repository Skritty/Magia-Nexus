using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Tile/Basic")]
public class GenerationTile : ScriptableObject
{
    [SerializeReference]
    public List<ITask<Vector3>> tasks;
}
