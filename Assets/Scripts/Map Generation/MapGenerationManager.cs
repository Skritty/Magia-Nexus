using System;
using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;
using Skritty.Tools.Utilities;

[Serializable]
public abstract class MapGenerator
{
    public abstract IEnumerator Generate(MapGenerationManager manager /*TODO: THIS IS TEMP*/, NTree<TileSuperposition> spatialTree, Bounds generationBounds, Action<MultidimensionalPosition> generateTile);
    public virtual void DrawGizmos(NTree<TileSuperposition> tree) {  }
}

public class MapGenerationManager : Singleton<MapGenerationManager>
{
    [NonSerialized]
    private NTree<TileSuperposition>[] mapRepresentationLODs;

    public int chunkBit = 3;
    public int chunkSize => 1 << chunkBit;
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
        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        mapRepresentationLODs = new NTree<TileSuperposition>[2];
        Generate(worldGeneration, 0, chunkGenBounds);
        Generate(mapGeneration, 1, initialTerrainGenBounds);
    }

    private void FixedUpdate()
    {
        Vector3 centerSnapped = new Vector3((int)center.position.x, (int)center.position.y, (int)center.position.z);
        if (centerSnapped != previousCenter || size != previousSize)
        {
            previousSize = size;
            previousCenter = centerSnapped;

            Bounds generationBounds = new Bounds(centerSnapped - transform.position, size);

            //Generate(mapGeneration, 1, generationBounds);
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
        MultidimensionalPosition chunkPos = position.PositionAtDepth(chunkBit) / chunkSize;
        chunkPos[1] = 0; // TODO: Remove for 3d chunks
        TileSuperposition tile = mapRepresentationLODs[0].GetDataAtPosition(chunkPos);
        if (tile == null) return null;
        List<GenerationTile> tiles = tile.GetTiles();
        if (tiles.Count == 0) return null;
        ChunkTile chunk = tiles[0] as ChunkTile;
        return chunk;
    }

    public List<(ChunkTile chunk, float distance)> GetNearbyChunks(MultidimensionalPosition position)
    {
        int halfChunk = (int)(chunkSize * 0.5f);
        List<(ChunkTile chunk, float distance)> chunks = new();
        RecursiveIteration(-1, true, new MultidimensionalPosition(position.Dimensions));
        return chunks;

        void RecursiveIteration(int axis, bool skip, MultidimensionalPosition offsetPosition)
        {
            if(axis >= 0)
            {
                if (skip) offsetPosition[axis] = position[axis];
                else offsetPosition[axis] = position[axis] + GetClosestChunkOffset(axis);
            }

            if (++axis < position.Dimensions)
            {
                RecursiveIteration(axis, true, offsetPosition);
                if (axis == 1) return; // TODO: remove this for 3d chunks
                RecursiveIteration(axis, false, offsetPosition);
            }
            else
            {
                ChunkTile chunk = GetChunk(offsetPosition);
                if (chunk == null) return;

                float dist = position.Distance(offsetPosition.PositionAtDepth(chunkBit) + halfChunk);
                float inverseDist = chunkSize - dist;
                chunks.Add((chunk, inverseDist));
            }
        }

        int GetClosestChunkOffset(int axis)
        {
            int offsetFromChunk = position[axis] % chunkSize;
            if (Math.Abs(offsetFromChunk) >= halfChunk)
            {
                return halfChunk * (offsetFromChunk < 0 ? -1 : 1);
            }
            else
            {
                return -halfChunk * (offsetFromChunk < 0 ? -1 : 1);
            }
        }
    }

    public void SaveMap() { }
    public void LoadMap() { }

    private void OnDrawGizmos()
    {
        if (mapRepresentationLODs == null) return;
        foreach (MapGenerator generator in worldGeneration.mapGenerators) if(generator != null) generator.DrawGizmos(mapRepresentationLODs[0]);
    }
}
