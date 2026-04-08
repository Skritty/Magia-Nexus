using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class MapGenerator
{
    public abstract IEnumerator Generate(MapGenerationManager manager /*TODO: THIS IS TEMP*/, NTree<TileSuperposition> spatialTree, Bounds generationBounds, Action<MultidimensionalPosition> generateTile);
    public virtual void DrawGizmos(NTree<TileSuperposition> tree) {  }
}

public class MapGenerationManager : Skritty.Tools.Utilities.Singleton<MapGenerationManager>
{
    [NonSerialized]
    private NTree<TileSuperposition>[] mapRepresentationLODs;

    public int chunkDimensions = 32;
    public Bounds chunkGenBounds;
    public Bounds initialTerrainGenBounds;
    public Transform center;
    public Vector3 size;
    public WFCTileGroup ErrorTile;
    public GenerationLayer worldGeneration;
    public GenerationLayer mapGeneration;
    public GenerationLayer levelGeneration;

    [Serializable]
    public class GenerationLayer
    {
        [SerializeReference]
        public List<MapGenerator> mapGenerators = new();
    }

    private Vector3 previousCenter, previousSize;

    private void Start()
    {
        //Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        mapRepresentationLODs = new NTree<TileSuperposition>[2];
        Generate(worldGeneration, 0, chunkGenBounds);
        /*Generate(mapGeneration, 1, initialTerrainGenBounds);
        WorldEventManager.Instance.Initialize();

        foreach (var node in mapRepresentationLODs[0].nodes)
        {
            WorldEventManager.Instance.GenerateChunkEvent(mapRepresentationLODs[1], node.position);
        }
        */
    }

    private void FixedUpdate()
    {
        if (center == null) return;
        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        if (centerSnapped != previousCenter || size != previousSize)
        {
            previousSize = size;
            previousCenter = centerSnapped;

            Bounds generationBounds = new Bounds(centerSnapped - transform.position, size);

            Generate(mapGeneration, 1, generationBounds);
            //Generate(levelGeneration, 1);
        }
    }

    private void Generate(GenerationLayer layer, int LODIndex, Bounds bounds)
    {
        if (layer == null) return;
        if(mapRepresentationLODs[LODIndex] == null)
        {
            mapRepresentationLODs[LODIndex] = new NTree<TileSuperposition>();
        }

        foreach (MapGenerator generator in layer.mapGenerators)
        {
            StartCoroutine(generator.Generate(this, mapRepresentationLODs[LODIndex], bounds, GenerateTileInternal));
        }

        void GenerateTileInternal(MultidimensionalPosition position)
        {
            List<GenerationTile> tile = mapRepresentationLODs[LODIndex][position].GetTiles();
            mapRepresentationLODs[LODIndex][position].generated = true;
            if (tile.Count == 0)
            {
                // Error
                //GenerateTile(ErrorTile.subtiles.objects[0,0,0], ErrorTile, tilePosition);
            }
            else if (tile.Count == 1)
            {
                GenerateTile(tile[0], position);
            }
        }
    }

    private bool GenerateTile(GenerationTile tile, MultidimensionalPosition position)
    {
        Vector3 pos = transform.position + new Vector3(position[0], position[1], position[2]);
        foreach (ITask<Vector3> task in tile.tasks)
        {
            task.DoTask(pos);
        }
        //if (tile.content == null) return false;
        //GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        //visuals.transform.position += transform.position + position.ToVector3;
            //- group.bounds.center + group.bounds.extents;
        //visuals.transform.parent = transform;
        return true;
    }

    public ChunkTile GetChunk(MultidimensionalPosition position)
    {
        MultidimensionalPosition chunkPos = new MultidimensionalPosition(position.Dimensions);
        for(int axis = 0; axis < chunkPos.Dimensions; axis++)
        {
            chunkPos[axis] = (ushort)(position[axis] * 1f / chunkDimensions);
        }
        TileSuperposition chunk = mapRepresentationLODs[0].GetDataAtPosition(chunkPos);
        if (chunk == null) return null;
        return chunk.GetTiles()[0] as ChunkTile;
    }

    public List<(ChunkTile chunk, int distance)> GetNearbyChunks(MultidimensionalPosition position)
    {
        List<(ChunkTile chunk, int distance)> chunks = new();
        for(int x = 0; x < 2; x++)
        {
            int xOffset = 0;
            if (x > 0) xOffset = (int)(chunkDimensions * 0.5f * (position[0] % chunkDimensions > chunkDimensions * 0.5f ? 1 : -1));
            for (int y = 0; y < 2; y++)
            {
                int yOffset = 0;
                if (y > 0) yOffset = (int)(chunkDimensions * 0.5f * (position[1] % chunkDimensions > chunkDimensions * 0.5f ? 1 : -1));
                for (int z = 0; z < 2; z++)
                {
                    int zOffset = 0;
                    if (z > 0) zOffset = (int)(chunkDimensions * 0.5f * (position[2] % chunkDimensions > chunkDimensions * 0.5f ? 1 : -1));

                    MultidimensionalPosition chunkPos = new MultidimensionalPosition(
                        (ushort)((position[0] + xOffset) / chunkDimensions), 
                        (ushort)((position[1] + yOffset) / chunkDimensions), 
                        (ushort)((position[2] + zOffset) / chunkDimensions));
                    TileSuperposition tile = mapRepresentationLODs[0].GetDataAtPosition(chunkPos);
                    if (tile == null) continue;
                    chunkPos[0] = (ushort)(chunkPos[0] * chunkDimensions + 0.5f * chunkDimensions);
                    chunkPos[1] = position[1];
                    chunkPos[2] = (ushort)(chunkPos[2] * chunkDimensions + 0.5f * chunkDimensions);
                    chunks.Add((tile.GetTiles()[0] as ChunkTile, (int)Mathf.Clamp(chunkDimensions - chunkPos.Distance(position), 0, chunkDimensions)));
                }
            }
        }
        return chunks;
    }

    public MultidimensionalPosition GetRandomChunkPosition(TileSuperposition filter)
    {
        // TODO: Add weighting to make this fancier
        List<MultidimensionalPosition> positions = new List<MultidimensionalPosition>();
        foreach(MultidimensionalPosition position in mapRepresentationLODs[0])
        {
            if (mapRepresentationLODs[0][position].Entropy == 1 && 
                filter.ContainsSubset(mapRepresentationLODs[0][position]))
                positions.Add(position);
        }
        UnityEngine.Random.InitState(filter.GetHashCode());
        return positions[UnityEngine.Random.Range(0, positions.Count)];
    }

    public ChunkTile GetRandomChunk(TileSuperposition filter)
    {
        return mapRepresentationLODs[0][GetRandomChunkPosition(filter)].GetTiles()[0] as ChunkTile;
    }

    public void SaveMap() { }
    public void LoadMap() { }

    private void OnDrawGizmos()
    {
        if (mapRepresentationLODs == null) return;
        foreach (MapGenerator generator in worldGeneration.mapGenerators) if(generator != null) generator.DrawGizmos(mapRepresentationLODs[0]);
        foreach (var node in mapRepresentationLODs[0].nodes)
        {
            Gizmos.DrawWireCube(node.position.ToVector3 * chunkDimensions + Vector3.one * (chunkDimensions * 0.5f - 0.5f) + transform.position, Vector3.one * chunkDimensions);
        }
    }
}
