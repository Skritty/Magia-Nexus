using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Tile/Chunk Info")]
public class ChunkTile : GenerationTile
{
    [SerializeReference]
    public List<NoisePass> terrainNoise = new();
    [SerializeReference]
    public List<GenerationRule> generationRules = new();
}