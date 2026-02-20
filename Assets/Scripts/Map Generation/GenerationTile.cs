using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Tile")]
public class GenerationTile : ScriptableObject
{
    public int index;
    public List<ITask<MultidimensionalPosition>> tasks;
}
