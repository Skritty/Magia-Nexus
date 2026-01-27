using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Skritty.Tools.Utilities;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class WFCMapGenerator : MonoBehaviour
{
    public Transform center;
    public int sqRadius, sqBufferSize;
    public List<WFCTileGroup> tileGroupPrefabs;
    [NonSerialized]
    public NTree<FlagList> spatialTree;
    public float creationDelay, solveDelay;
    public WFCTileGroup ErrorTile;

    private EntropicSet<MultidimensionalPosition> entropicMap;
    private HashSet<MultidimensionalPosition> bufferTiles = new();
    private HashSet<MultidimensionalPosition> generatedTiles = new();
    private Queue<MultidimensionalPosition> updateQueue = new();
    private HashSet<MultidimensionalPosition> inQueue = new();
    

    [SerializeField]
    private List<WFCTileGroup> tileGroups = new();
    private List<WFCTile> tileOptions;
    private ulong defaultTileFlags;
    private Dictionary<string, WFCTileGroup> tileGroupsByUID = new();
    private WeightedChance<WFCTile> potentialTiles = new();
    private int tileCount;
    private ulong allowed;
    private (int x, int y, int z) updating;
    private const ulong ULOne = 1;
    private bool generating;
    private Vector3 targetCenter = Vector3.zero;
    private Vector3 previousCenter = Vector3.zero;

    public class FlagList
    {
        public ulong options;
        public int Entropy
        {
            get
            {
                if (options == 0) return 0;
                int entropy = 0;
                for (int i = 0; i < 64; i++)
                {
                    if ((options & (ULOne << i)) != 0) entropy++;
                }
                return entropy;
            }
        }

        public bool Solved => (options & (options - 1)) == 0;

        public FlagList(ulong options)
        {
            this.options = options;
        }

        public List<T> GetObjects<T>(params T[] objects)
        {
            List<T> objectsList = new();
            for(int i = 0; i < objects.Count(); i++)
            {
                if ((options & (ULOne << i)) != 0)
                {
                    objectsList.Add(objects[i]);
                }
            }
            return objectsList;
        }
    }
    
    private void Start()
    {
        SolveConnections();
        Initialize();
        targetCenter = center.position - transform.position;
    }

    private void FixedUpdate()
    {
        if (!generating)
        {
            targetCenter = Vector3.Lerp(targetCenter, center.position - transform.position, 0.01f);
            
            if(Vector3.Distance(previousCenter, targetCenter) >= 1)
            {
                StartCoroutine(GenerateMap());
                previousCenter = targetCenter;
            }
        }
    }

    //[InfoBox("Connections must be resolved any time a change is made to subtile dimensions or if the tile group list is changed"), Button("Solve Connections")]
    public void SolveConnections()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        tileGroups.Clear();
        tileGroupsByUID = new();
        List<WFCTile> allTiles = new List<WFCTile>();

        // Create prefab instances, put them into UID dictionary
        foreach (WFCTileGroup groupPrefab in tileGroupPrefabs)
        {
            WFCTileGroup loadedTileGroup = Instantiate(groupPrefab);
            loadedTileGroup.gameObject.SetActive(false);
            loadedTileGroup.transform.parent = transform;
            tileGroups.Add(loadedTileGroup);
            tileGroupsByUID.Add(loadedTileGroup.groupUID, loadedTileGroup);
            foreach (WFCTile tile in loadedTileGroup.subtiles)
            {
                if (tile.isHole) continue;
                allTiles.Add(tile);
                tile.tileBit = ULOne << tileCount;
                tileCount++;
            }
        }

        // Set up connections with loaded tile groups
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            // Handle internal connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (tile.weight == 0) tile.weight = tileGroup.weight;

                WFCTile[] adjecentTiles = tileGroup.subtiles.GetAdjecentObjects(tile);
                for (int i = 0; i < 6; i++)
                {
                    if (adjecentTiles[i] == null) continue;
                    if (adjecentTiles[i].isHole)
                    {
                        tile.connections[i].allowedTileRefs.AddRange(adjecentTiles[i].holeAllowedTileRefs);
                        tile.connections[i].isInternalConnection = false;
                    }
                    else
                    {
                        tile.connections[i].allowedTiles |= adjecentTiles[i].tileBit;
                        tile.connections[i].isInternalConnection = true;
                    }
                }
            }

            // Handle group links
            /*foreach (WFCTile tile in tileGroup.subtiles)
            {
                for (int i = 0; i < 6; i++)
                {
                    foreach (WFCTileRef connectedTileRef in tile.connections[i].allowedTileRefs.ToArray())
                    {
                        if (!tileGroupsByUID.ContainsKey(connectedTileRef.groupUID)) continue;
                        // Find the index where the connecting tile would be
                        (int, int, int) projectedIndex = tileGroup.subtiles.GetIndex(tile);
                        projectedIndex = (projectedIndex.Item1 + (i == 0 || i == 1 ? -(i % 2 * 2 - 1) : 0),
                            projectedIndex.Item2 + (i == 2 || i == 3 ? -(i % 2 * 2 - 1) : 0),
                            projectedIndex.Item3 + (i == 4 || i == 5 ? -(i % 2 * 2 - 1) : 0));
                        ConjoinTileGroups(tileGroup, tileGroupsByUID[connectedTileRef.groupUID], projectedIndex, (connectedTileRef.x, connectedTileRef.y, connectedTileRef.z));
                    }
                }
            }*/

            // Handle connections
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (tile.isHole) continue;
                // Set up and reciprocate actual tile connections from tileRefs
                for (int i = 0; i < 6; i++)
                {
                    if (tile.connections[i].isInternalConnection) continue;
                    int reciprocatedIndex = i + (i % 2 > 0 ? -1 : 1);
                    if (tile.connections[i].allowedTileRefs.Count == 0)
                    {
                        // Wildcard tile
                        foreach (WFCTile tileActual in allTiles)
                        {
                            WFCConnection otherConnection = tileActual.connections[reciprocatedIndex];
                            if (otherConnection.isInternalConnection) continue;

                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            otherConnection.allowedTiles |=  tile.tileBit;
                        }
                    }
                    else
                    {
                        foreach (WFCTileRef connectedTileRef in tile.connections[i].allowedTileRefs)
                        {
                            if (!tileGroupsByUID.ContainsKey(connectedTileRef.groupUID))
                            {
                                // Tile group referened doesn't exist in this generation
                                continue;
                            }
                            WFCTile tileActual = tileGroupsByUID[connectedTileRef.groupUID].subtiles[connectedTileRef.x, connectedTileRef.y, connectedTileRef.z];
                            tile.connections[i].allowedTiles |= tileActual.tileBit;
                            tileActual.connections[reciprocatedIndex].allowedTiles |= tile.tileBit;
                        }
                    }
                }
            }
        }
    }

    private void ConjoinTileGroups(WFCTileGroup group1, WFCTileGroup group2, (int, int, int) projectedIndex, (int, int, int) initialConnectionIndex)
    {
        // Iterate through group1 subtiles (plus 1 around)
        for (int x = -1; x <= group1.subtiles.x; x++)
        {
            for (int y = -1; y <= group1.subtiles.y; y++)
            {
                for (int z = -1; z <= group1.subtiles.z; z++)
                {
                    (int, int, int) connectionIndex = (
                        initialConnectionIndex.Item1 - projectedIndex.Item1 + x,
                        initialConnectionIndex.Item2 - projectedIndex.Item2 + y,
                        initialConnectionIndex.Item3 - projectedIndex.Item3 + z);

                    if ((uint)connectionIndex.Item1 >= group2.subtiles.x
                        || (uint)connectionIndex.Item2 >= group2.subtiles.y
                        || (uint)connectionIndex.Item3 >= group2.subtiles.z)
                        continue;

                    if(group2.subtiles[connectionIndex].isHole) continue;

                    WFCTile selfTile = null;
                    if ((uint)x < group1.subtiles.x && (uint)y < group1.subtiles.y && (uint)z < group1.subtiles.z)
                    {
                        selfTile = group1.subtiles[x, y, z];
                    }

                    if (selfTile == null || selfTile.isHole)
                    {
                        WFCTile[] holeAdjacentTiles = group1.subtiles.GetAdjecentObjects((x, y, z));
                        for (int i = 0; i < 6; i++)
                        {
                            if (holeAdjacentTiles[i] == null || holeAdjacentTiles[i].isHole) continue;

                            WFCConnection connection = holeAdjacentTiles[i].connections[i + (i % 2 > 0 ? -1 : 1)];

                            WFCTileRef tileRef = new WFCTileRef(group2.groupUID, connectionIndex.Item1, connectionIndex.Item2, connectionIndex.Item3);
                            if (!connection.allowedTileRefs.Any(x => x.groupUID == tileRef.groupUID)) connection.allowedTileRefs.Add(tileRef);

                            //Debug.Log($"Connecting {group2.gameObject.name} ({projectedIndex}) to {gameObject.name} ({initialConnectionIndex}) at connection index {i + (i % 2 > 0 ? -1 : 1)} | ");
                            // xyz = (0,0,-1), initialSelfIndex = (0,0,-1)
                            // initialConnectionIndex = (0,0,0), goalIndex = (1,0,0)
                            // initialConnectionIndex - initialSelfIndex + xyz = goalIndex
                        }
                    }
                    else
                    {
                        // Overlaps!
                        Debug.LogWarning($"{gameObject.name} overlaps with connection {group2.gameObject.name}");
                        return;
                    }
                }
            }
        }
    }

    public void Initialize()
    {
        spatialTree = new();
        
        tileOptions = new();
        tileGroupsByUID = new();
        tileGroupsByUID.Add(ErrorTile.groupUID, ErrorTile);

        // Add all valid tiles in each group to the list of possible tiles.
        foreach (WFCTileGroup tileGroup in tileGroups)
        {
            tileGroupsByUID.Add(tileGroup.groupUID, tileGroup);
            foreach (WFCTile tile in tileGroup.subtiles)
            {
                if (!tile.isHole)
                {
                    tileOptions.Add(tile);
                }
            }
        }

        entropicMap = new(tileOptions.Count);
        defaultTileFlags = ulong.MaxValue >> Mathf.Clamp(64 - tileOptions.Count, 0, 64);
    }

    private FlagList AddTile(MultidimensionalPosition position)
    {
        FlagList flagList = new(defaultTileFlags);
        spatialTree.TryAddData(flagList, position, out _, true);
        if (!IsGenerationTile(position))
        {
            // Is the tile outside generation range?
            bufferTiles.Add(position);
        }
        else
        {
            entropicMap.Add(tileOptions.Count, position);
        }
        return flagList;
    }

    public IEnumerator GenerateMap()
    {
        generating = true;

        /*if (center.x - transform.position.x < 0
            || center.y - transform.position.y < 0
            || center.z - transform.position.z < 0) return null;*/

        /*MultidimensionalPosition generationStartPos = new(
            (ushort)(center.x - transform.position.x),
            (ushort)(center.y - transform.position.y),
            (ushort)(center.z - transform.position.z));*/

        MultidimensionalPosition generationStartPos = new(
           (ushort)(targetCenter.x),
           (ushort)(targetCenter.y),
           (ushort)(targetCenter.z));

        // Set up seed nodes
        if (spatialTree[generationStartPos] == null)
        {
            AddTile(generationStartPos);
        }
        else
        {
            foreach (MultidimensionalPosition position in bufferTiles.ToArray())
            {
                if (!IsGenerationTile(position)) continue;
                entropicMap.Add(spatialTree[position].Entropy, position);
                bufferTiles.Remove(position);
            }
        }

        int tilesPerDelay = 25;
        int tilesPlaced = 0;
        // Solve the WFC
        while (entropicMap.Count > 0)
        {
            MultidimensionalPosition position = entropicMap.GetRandomAtLowestEntropy();
            entropicMap.Remove(position);

            potentialTiles.Clear();
            foreach (WFCTile tile in spatialTree[position].GetObjects(tileOptions.ToArray()))
            {
                potentialTiles.Add(tile, tile.weight);
            }
            WFCTile selectedTile = potentialTiles.GetRandomEntry();

            spatialTree[position].options = ULOne << tileOptions.IndexOf(selectedTile);
            updateQueue.Enqueue(position);
            inQueue.Add(position);
            if(GenerateTile(selectedTile, tileGroupsByUID[selectedTile.groupUID], position))
            {
                tilesPlaced++;
                if (tilesPlaced >= tilesPerDelay && creationDelay > 0)
                {
                    tilesPlaced = 0;
                    yield return new WaitForSeconds(creationDelay);
                }
            }
            while (updateQueue.Count > 0)
            {
                position = updateQueue.Dequeue();
                inQueue.Remove(position);
                Observe(position);
                if (solveDelay > 0) yield return new WaitForSeconds(solveDelay);
            }
        }

        // Instantiate tile visuals
        /*for (int x = 0; x < mapRepresentation.x; x++)
        {
            for (int y = 0; y < mapRepresentation.y; y++)
            {
                for (int z = 0; z < mapRepresentation.z; z++)
                {
                    if (mapRepresentation[x, y, z].Count != 1) continue;
                    WFCTile tile = mapRepresentation[x, y, z].First();
                    WFCTileGroup group = tileGroupsByUID[tile.groupUID];
                    if (tile.content == null) continue;
                    GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
                    visuals.transform.position += transform.position + new Vector3(x, y, z)
                        - group.bounds.center + group.bounds.extents;
                    visuals.transform.parent = transform;
                }
            }
        }*/

        /*foreach (WFCTileGroup tileGroup in tileGroups)
        {
            Destroy(tileGroup.gameObject);
        }
        tileGroups.Clear();*/
        generating = false;
        //Debug.Break();
    }

    private bool IsBufferTile(MultidimensionalPosition position)
    {
        float sqDist = Mathf.Pow(targetCenter.x - position[0], 2) + Mathf.Pow(targetCenter.y - position[1], 2) + Mathf.Pow(targetCenter.z - position[2], 2);
        return sqDist > sqRadius && sqDist <= sqRadius + sqBufferSize;
    }

    private bool IsGenerationTile(MultidimensionalPosition position)
    {
        float sqDist = Mathf.Pow(targetCenter.x - position[0], 2) + Mathf.Pow(targetCenter.y - position[1], 2) + Mathf.Pow(targetCenter.z - position[2], 2);
        return sqDist <= sqRadius;
    }

    private void Observe(MultidimensionalPosition position)
    {
        FlagList tiles = spatialTree[position];
        if (position[0] != ushort.MaxValue) Collapse(tiles, 0, new((ushort)(position[0] + 1), position[1], position[2]));
        if (position[0] != 0) Collapse(tiles, 1, new((ushort)(position[0] - 1), position[1], position[2]));
        if (position[1] != ushort.MaxValue) Collapse(tiles, 2, new(position[0], (ushort)(position[1] + 1), position[2]));
        if (position[1] != 0) Collapse(tiles, 3, new(position[0], (ushort)(position[1] - 1), position[2]));
        if (position[2] != ushort.MaxValue) Collapse(tiles, 4, new(position[0], position[1], (ushort)(position[2] + 1)));
        if (position[2] != 0) Collapse(tiles, 5, new(position[0], position[1], (ushort)(position[2] - 1)));
    }

    private ulong GetAllowedTiles(FlagList tiles, int connectionIndex)
    {
        allowed = 0;
        foreach (WFCTile tile in tiles.GetObjects(tileOptions.ToArray()))
        {
            allowed |= tile.connections[connectionIndex].allowedTiles;
        }
        return allowed;
    }

    private void Collapse(FlagList possibleConnections, int connectionIndex, MultidimensionalPosition position)
    {
        FlagList potentialTiles = spatialTree[position];
        // Is this tile outside of the map or is it already solved?
        if (potentialTiles == null)
        {
            potentialTiles = AddTile(position);
        }
        if(potentialTiles.Solved) return;

        ulong allowedTiles = GetAllowedTiles(possibleConnections, connectionIndex);
        //updating = index;
        ulong options = potentialTiles.options;
        potentialTiles.options &= allowedTiles;

        if (options != potentialTiles.options)
        {
            if (potentialTiles.options == 0)
            {
                // Error! No tile options left to pick
                GenerateTile(ErrorTile.subtiles[0, 0, 0], ErrorTile, position);
                entropicMap.Remove(position);
                return;
            }

            if (potentialTiles.Solved)
            {
                // Tile is determined
                WFCTile tile = potentialTiles.GetObjects(tileOptions.ToArray())[0];
                GenerateTile(tile, tileGroupsByUID[tile.groupUID], position);
                entropicMap.Remove(position);
            }
            else
            {
                // Tile entropy reduced
                entropicMap.Update(potentialTiles.Entropy, position);
            }

            // Add to the update queue
            if (!inQueue.Contains(position) && (IsGenerationTile(position) || IsBufferTile(position)))
            {
                updateQueue.Enqueue(position);
                inQueue.Add(position);
            }
        }
    }

    private bool GenerateTile(WFCTile tile, WFCTileGroup group, MultidimensionalPosition position)
    {
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Clear();
        //mapRepresentation[index.Item1, index.Item2, index.Item3].Add(tile);

        if (tile.content == null) return false;
        GameObject visuals = Instantiate(tile.content, Vector3.zero, Quaternion.identity);
        visuals.transform.position += transform.position + position.ToVector3
            - group.bounds.center + group.bounds.extents;
        visuals.transform.parent = transform;
        generatedTiles.Add(position);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (spatialTree == null || tileGroups == null) return;
        foreach (var node in spatialTree.nodes)
        {
            MultidimensionalPosition position = node.position;
            FlagList tiles = spatialTree[position];
            /*if (updating.Item1 == position[0] && updating.Item2 == position[1] && updating.Item3 == position[2])
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(position.ToVector3 + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
            }*/
            //else
            {
                if (!IsGenerationTile(position)) continue;
                if (tiles.Solved)
                {
                    Gizmos.color = Color.blue;
                    continue;
                }
                else
                {
                    float a = (1f - tiles.Entropy * 1f / tileCount);
                    if (a == 0) continue;
                    Gizmos.color = new Color(a, a, a, a * 0.2f);
                }

                Gizmos.DrawCube(new Vector3(position[0], position[1], position[2]) + transform.position + Vector3.one * 0.5f, Vector3.one * 0.9f);
            }
        }
    }
}

