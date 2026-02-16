using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class NTree<T>
{
    public class NTreeNode
    {
        public static HashSet<NTreeNode> allNodes = new HashSet<NTreeNode>();

        public int id; // Index to the reference of data stored in this leaf
        public MultidimensionalPosition position;
        public byte depth;

        [HideInInspector]
        public NTreeNode parent;
        public int IndexInParent => position.GetIndexAtDepth(depth);
        [HideInInspector]
        public NTreeNode[] children = new NTreeNode[0];
        public byte childCount = 0;
        // Index: x,y,z bit for this position
        // 0: 0,0,0 | 1: 1,0,0
        // 2: 0,1,0 | 3: 1,1,0
        // 4: 0,0,1 | 5: 1,0,1
        // 6: 0,1,1 | 7: 1,1,1

        public NTreeNode(MultidimensionalPosition position, int id = -1, byte depth = 0)
        {
            Initialize(position, id, depth);
        }

        public NTreeNode Initialize(MultidimensionalPosition position, int id = -1, byte depth = 0)
        {
            allNodes.Add(this);
            this.id = id;
            this.depth = depth;
            if (depth > 0)
            {
                // Path node
                children = new NTreeNode[1 << position.Dimensions];
                this.position = position & (~0 << depth);
            }
            else
            {
                // Leaf node
                this.position = position.Clone();
            }
            return this;
        }

        /// <summary>
        /// Update the position of this node
        /// </summary>
        public void UpdatePosition(MultidimensionalPosition position)
        {
            if (parent == null) return; // Cannot move something that is not attached
            parent.children[IndexInParent] = null;
            this.position = position &= (~0 << depth);
            parent.children[IndexInParent] = this;
        }

        public NTreeNode Reset()
        {
            depth = 0;
            parent = null;
            children = new NTreeNode[0];
            return this;
        }

        public NTreeNode AddChild(NTreeNode node)
        {
            node.parent = this;
            children[node.IndexInParent] = node;
            childCount++;
            return node;
        }

        public void RemoveChild(NTreeNode node)
        {
            node.parent = null;
            children[node.IndexInParent] = null;
            childCount--;
        }

        public NTreeNode GetLeaf(MultidimensionalPosition position, byte targetDepth)
        {
            if (depth == targetDepth) return this;
            NTreeNode child = children[position.GetIndexAtDepth(depth - 1)];
            if (child == null) return null;
            return child.GetLeaf(position, targetDepth);
        }
        public override string ToString()
        {
            return $"({position[0]},{position[1]},{position[2]})";
        }

        /// <summary>
        /// Makes a path from one node to another node along the given positional route
        /// </summary>
        public void AddSector(NTreeNode end, MultidimensionalPosition position)
        {
            // Check if that node already exists, if not, make it
            NTreeNode node = children[position.GetIndexAtDepth(depth - 1)];
            if (node == null)
            {
                if ((depth - 1) == end.depth)
                {
                    // Add the end point
                    AddChild(end);
                }
                else
                {
                    // If no path exists, add a new path node and traverse it
                    NTreeNode pathNode = CreateNode(position, 0, (byte)(depth - 1));
                    AddChild(pathNode);
                    pathNode.AddSector(end, position);
                }
            }
            else if (node.depth == end.depth)
            {
                // If this node already exists and is the end point (not empty), put the extra node into the pool and merge their children
                node.Merge(end);
            }
            else
            {
                // Continue traversing down the path
                node.AddSector(end, position);
            }
        }

        /// <summary>
        /// Merges other into this
        /// </summary>
        public void Merge(NTreeNode other)
        {
            // Remove other from its parent (Ensure there are no infinite loops)
            if (other.parent != null)
            {
                other.parent.children[other.IndexInParent] = null;
            }

            foreach (NTreeNode child in other.children)
            {
                if (child == null) continue;
                if (children[child.IndexInParent] == null)
                {
                    // If this child doesn't exist yet, add the one from other
                    AddChild(child);
                    //Debug.Log($"(I{indexInParent}, D{depth})  merged {child.leafCount} into {leafCount - child.leafCount}, now at {leafCount} leafs");
                }
                else
                {
                    // If this child does exist, merge those two children
                    children[child.IndexInParent].Merge(child);
                }
            }
            ReleaseNode(other);
        }

        /*public void GetChildrenInRadius(ref int nearby, MultidimensionalPosition start, int radius)
        {
            // (7,12) in radius of 4
            // min: (3,8), max: (11,16)
            // FIGURE OUT MIN AND MAX BASED ON DIRECTION FROM START NODE
            MultidimensionalPosition closestLeaf = new MultidimensionalPosition(position.dimensions);
            MultidimensionalPosition farthestLeaf = new MultidimensionalPosition(position.dimensions);
            int distClosest = 0;
            int distFarthest = 0;
            for (int axis = 0; axis < position.dimensions; axis++)
            {
                if(position[axis] == start[axis])
                {
                    closestLeaf[axis] = position[axis];
                    farthestLeaf[axis] = position[axis];
                }
                else if(position[axis] > start[axis])
                {
                    // closest is minimium
                    closestLeaf[axis] = position[axis];
                    farthestLeaf[axis] = (ushort)(position[axis] & depthMask);
                }
                else
                {
                    // farthest is minimum
                    closestLeaf[axis] = (ushort)(position[axis] & depthMask);
                    farthestLeaf[axis] = position[axis];
                }
            }
            MultidimensionalPosition radialContribution = new MultidimensionalPosition(position.dimensions);
            int radius = 0;

            if (position[axis] >= minPos[axis] && (position[axis] & depthMask) <= maxPos[axis])
            {
                // Fully contained
                nearby |= leafIndex;
                return;
            }
            else if (position[axis] <= maxPos[axis]) // Min in range
            {
                // Partially Contained
                Descend(ref nearby);
            }
            else if ((position[axis] & depthMask) >= minPos[axis]) // Max in range
            {
                // Partially Contained
                Descend(ref nearby);
            }
            else if (minPos[axis] > position[axis] && maxPos[axis] < (position[axis] & depthMask)) // Contained
            {
                // Check if partial contained within
                Descend(ref nearby);
            }

            void Descend(ref int nearby)
            {
                foreach (NTreeNode node in children)
                {
                    if (node != null) GetChildrenInRadius(ref nearby, minPos, maxPos);
                }
            }
        }*/

        public void AscendSearch(List<NTreeNode> nearby, MultidimensionalPosition startPosition, int radius, byte dimensionMask, int exclusionIndex)
        {
            if(depth != 0)
            {
                bool skip = false;

                // Get farthest possible position within this node from the start node
                // current path postion: 00000 = 0
                // start position:       00101 = 5
                //                       0~-->
                // farthest position:    01111 = 15
                MultidimensionalPosition farthest = startPosition.FarthestPositionAtDepth(depth - 1);

                // Compress each axis to match the dimension mask
                farthest.MaskDimensions(dimensionMask);

                // Check if the farthest node is in the radius (this means all nodes are)
                if (startPosition.GetDistApprox(farthest) <= radius)
                {
                    skip = true;
                }

                for (int i = 0; i < children.Length; i++)
                {
                    if (i == exclusionIndex) continue;
                    children[i].DescendSearch(nearby, startPosition, radius, dimensionMask, skip);
                }
            }
            
            if(parent != null)
            {
                AscendSearch(nearby, startPosition, radius, dimensionMask, IndexInParent);
            }
        }

        private void DescendSearch(List<NTreeNode> nearby, MultidimensionalPosition startPosition, int radius, byte dimensionMask, bool skip)
        {
            if(depth == 0)
            {
                // Check if within distance
                if (!skip && startPosition.GetDistApprox(position) > radius) return;
                nearby.Add(this);
                return;
            }

            if (!skip)
            {
                // Check if the farthest node is in the radius (this means all nodes are)
                MultidimensionalPosition farthest = startPosition.FarthestPositionAtDepth(depth - 1);
                farthest.MaskDimensions(dimensionMask);
                if (startPosition.GetDistApprox(farthest) <= radius)
                    skip = true;

                if (!skip)
                {
                    // Check to see if the closest node is in the radius (this means no nodes are)
                    MultidimensionalPosition closest = startPosition.ClosestPositionAtDepth(depth - 1);
                    closest.MaskDimensions(dimensionMask);
                    if (startPosition.GetDistApprox(closest) <= radius)
                    {
                        return;
                    }
                }
            }

            foreach (NTreeNode child in children)
            {
                child.DescendSearch(nearby, startPosition, radius, dimensionMask, skip);
            }
        }

        // Radius is silly, it is x+y+z and NOT the *actual* distance away from a node TODO: figure out a way to make this an accurate circle through magic
        /*public void GetChildrenInRadius(List<ushort> nearby, int radius, MultidimensionalPosition start, byte descentStartDepth, int prevChildIndex, bool skip)//, Action<T, ushort, int[]> calculate)
        {
            // Leaf
            if (depth == 0)
            {
                nearby.Add(id);
                // First node go up
                if (nearby.Count == 1)
                {
                    parent.GetChildrenInRadius(nearby, radius, position, 1, indexInParent, false);//, calculate);
                }
                else
                {
                    int[] dir = new int[start.dimensions];
                    for (int axis = 0; axis < start.dimensions; axis++)
                    {
                        dir[axis] = position[axis] - start[axis];
                    }
                    //Debug.Log($"#2 Distance Final: {dir[0] + dir[1] + dir[2]} | {start} -> {position}");
                    //calculate?.Invoke(nearby[0], id, dir);
                }
                return;
            }

            if(leafCount > 1 || (prevChildIndex == -1 && leafCount > 0))
            {
                int dist = 0;
                // Global check to see if all nodes under this are within the radius (skipping further checks down the line)
                if (!skip)
                {
                    for (int axis = 0; axis < start.dimensions; axis++)
                    {
                        if ((start[axis] & (1 << depth - 1)) != 0) // 1, dist is the position (excluding any bits above the child depth)
                        {
                            dist += start[axis] & ~(~0 << depth);
                        }
                        else // 0, dist is the opposite of position (excluding any bits above the child depth)
                        {
                            dist += ~start[axis] & ~(~0 << depth);
                        }
                    }
                    if (dist <= radius) skip = true;
                }

                //bool done = true;
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    if ((prevChildIndex != -1 && prevChildIndex == childIndex) || children[childIndex] == null) continue;

                    if (skip)
                    {
                        //Debug.Log($"#2 Distance: in radius | Depth {depth} | {start} -> {children[childIndex].position}");
                        children[childIndex].GetChildrenInRadius(nearby, radius, start, descentStartDepth, -1, skip);//, calculate);
                        continue;
                    }

                    // First number is the start node, the -> is the closest node, [] is the child we are currently observing (this bit can be either 0 or 1), and in the () is how we find the closest node
                    //             bits below child => [ ] these bits are all either 1 or 0, whatever is closest to the starting node. If the child is a leaf, skip inversion check
                    // 110 -> 0[1]1 (01[0] invert to 01[1] because highest point is [1]10), distance = 110 (6) - 011 (3) = 3
                    // 110 -> 0[0]1 (00[0] invert to 00[1] because highest point is [1]10), distance = 110 (6) - 001 (1) = 5
                    // 110 -> 00[0], distance = 110 (6) - 000 (0) = 6
                    // 010 -> 1[0]0 (10[0] remain the same because highest point is [0]10), distance = 010 (2) - 100 (4) (flip required first) = 2
                    // 1010 -> 00[1]1 (000[0] invert to 001[1] because highest point is [1]010), distance = 1010 (10) - 0011 (3) = 7
                    // Needed info: start position, current position down (bit is set to the child's bit position when descending), descent start depth (to find the highest point)
                    // Check if the closest possible node in this child is within the radius
                    int closestPos = 0;
                    int distance = 0;
                    for (int axis = 0; axis < start.dimensions; axis++)
                    {
                        if (depth > 1)
                        {
                            //Debug.Log($"Descent Start: {descentStartDepth}, start: {start[axis]}({(start[axis] & (1 << descentStartDepth)) != 0}), current: {children[childIndex].position[axis]}({(children[childIndex].position[axis] & (1 << descentStartDepth)) != 0})");
                            if ((start[axis] & (1 << descentStartDepth)) == (children[childIndex].position[axis] & (1 << descentStartDepth)))
                            {
                                // If no change, ignore change in position
                                continue;
                            }
                            else if ((start[axis] & (1 << descentStartDepth)) != 0)
                            {
                                closestPos = (children[childIndex].position[axis] | ~(~0 << depth - 1));
                            }
                            else
                            {
                                closestPos = children[childIndex].position[axis] & (~0 << depth - 1);
                            }
                        }
                        else
                        {
                            // Actual position of the leaf
                            closestPos = children[childIndex].position[axis];
                        }

                        distance += Math.Abs(start[axis] - closestPos);
                    }

                    //Debug.Log($"#2 Distance: {distance} | Depth: {depth} | {start} -> {children[childIndex].position}");
                    if (distance <= radius)
                    {
                        //done = false;
                        children[childIndex].GetChildrenInRadius(nearby, radius, start, descentStartDepth, -1, skip);//, calculate);
                    }
                }
            }

            

            // Go up if there is a parent to go to, if we are not currently going down (we've already visted the parent), or if there was at least one valid child found under this node (to stop early if there is nothing else in radius)
            if (parent != null && prevChildIndex != -1)// && (skip || !done))
            {
                parent.GetChildrenInRadius(nearby, radius, start, ++descentStartDepth, indexInParent, false);//, calculate);
            }
        }*/

        // We have ascending/descending/branch offsets to track our current progess up/down/over without needing to make new arrays. These arrays are made once and shared for the entire recursion
        /*public void GetChildrenInRadius(List<ushort> nearby, int radius, int upBitMask, int downBitMask, MultidimensionalPosition offset, MultidimensionalPosition currentPosition, bool up, int prevChildIndex, int[] posOut)//, Action<T, T, int[]> calculate)
        {
            if(children != null)
                foreach(NTreeNode n in children)
                    if(n != null) maximumDown++;
            // If this is a leaf, add it to the list
            if (depth == 0)
            {
                nearby.Add(id);
                //Debug.Log($"{(data as Simulator.SimulatedNode).id} | {nearby.Count}");
                // First node go up
                if (nearby.Count == 1)
                {
                    GoUp();
                }
                else
                {
                    //Debug.Log($"#1 Distance Final: {posOut[0] + posOut[1] + posOut[2]}");
                    //calculate?.Invoke(nearby[0], data, posOut);
                }
                return;
            }

            
            int distance = 0;
            // Check children
            //Debug.Log($"UpBitMask: {Convert.ToString(upBitMask, 2)}, DownBitMask: {Convert.ToString(downBitMask, 2)}");
            for (int childIndex = 0; childIndex < children.Length; childIndex++)
            {
                if ((prevChildIndex != -1 && prevChildIndex == childIndex) || children[childIndex] == null) continue;

                distance = 0;
                for (int axis = 0; axis < offset.dimensions; axis++)
                {
                    // Set the bit of currentPosition to the bit of this child at the bitPosition depth-1
                    if (((currentPosition[axis] >> depth - 1) & 1) != ((childIndex >> axis) & 1)) // 0->1, 10->00, 00->00, 00->01, 100->000, 000->000, 000->000, 001->001
                        currentPosition[axis] ^= (ushort)(1 << depth - 1);

                    // Check if there was a change from the emerging node to the new node
                    if ((offset[axis] & downBitMask) != (currentPosition[axis] & downBitMask))
                    {
                        if ((offset[axis] & downBitMask) > (currentPosition[axis] & downBitMask))
                        {
                            // 1->0 assume all bits below this depth are 1
                            posOut[axis] = offset[axis] - (currentPosition[axis] | (upBitMask >> 1)); // 2-1=1, 2-0=2, 2-1=1, 6-3=3, 6-1=5, 6-0=6, 6-1=5
                            distance += posOut[axis];
                            posOut[axis] *= -1;
                            //Debug.Log($"Offset: {offset[axis]}, Position: {currentPosition[axis]} | {offset[axis]} - {(currentPosition[axis] | (upBitMask >> 1))}");
                        }
                        else
                        {
                            // 0->1 assume all bits below this depth are 0
                            distance += posOut[axis] = (currentPosition[axis] & ~(upBitMask >> 1)) - offset[axis]; // 1-0=1
                            //Debug.Log($"Offset: {offset[axis]}, Position: {currentPosition[axis]} | {(currentPosition[axis] & ~(upBitMask >> 1))} - {offset[axis]}");
                        }
                    }
                    else
                    {
                        posOut[axis] = 0;
                        //Debug.Log($"Offset: {offset[axis]}, Position: {currentPosition[axis]} | No Change");
                    }
                    //Debug.Log($"#1 Axis: {axis} | Dist: {posOut[axis]}");
                }

                //Debug.Log($"Depth: {depth} | Distance: {distance}");
                //Debug.Log($"#1 Distance: {distance} | Depth: {depth}");
                // Go down if within radius
                if (distance <= radius)
                {
                    GoDown(childIndex);
                }
            }

            // Go up if we aren't going down and if we haven't hit the root (FIND A WAY TO MAKE THIS STOP EARLY IF THERE IS NOTHING ELSE THAT COULD BE IN THE RADIUS)
            if (up && parent != null)
            {
                GoUp();
            }

            void GoUp()
            {
                for (int axis = 0; axis < offset.dimensions; axis++)
                {
                    if((indexInParent & 1 << axis) != 0)
                    {
                        currentPosition[axis] = (ushort)(1 << depth); // 10, 100
                        offset[axis] |= currentPosition[axis]; // 10, 110
                    }
                    else
                    {
                        currentPosition[axis] = 0; // 0
                        // offset; // 0
                    }
                }
                //Debug.Log($"Offset: ({offset[0]},{offset[1]},{offset[2]})");
                //upBitMask; // 1, 11, 111
                //downBitMask; // 1, 10, 100
                parent.GetChildrenInRadius(nearby, radius, upBitMask | 1 << depth, 1 << depth, offset, currentPosition, true, indexInParent, posOut);//, calculate);
            }

            void GoDown(int childIndex)
            {
                totalDown++;
                for (int axis = 0; axis < offset.dimensions; axis++)
                {
                    if((childIndex & 1 << axis) != 0)
                        currentPosition[axis] |= (ushort)(1 << depth - 1);
                    else
                        currentPosition[axis] &= (ushort)~(1 << depth - 1); // 00, 000, 000
                }
                //offset; // 10, 110, 110
                //upBitMask; // 01, 011, 001
                //downBitMask; // 10, 110, 111
                children[childIndex].GetChildrenInRadius(nearby, radius, upBitMask >> 1, downBitMask | 1 << depth - 1, offset, currentPosition, false, -1, posOut);//, calculate);
            }
        }*/
    }

    private NTreeNode root;
    private byte depth; // 0 is the lowest depth, don't complain
    private int highestDimensions; // Always create new roots in the highest dimension in the tree

    public List<T> data = new();
    public List<NTreeNode> nodes = new();
    public static Queue<NTreeNode> nodePool = new Queue<NTreeNode>();

    static int totalDown = 0;
    static int maximumDown = 0;

    public T this[MultidimensionalPosition position]
    {
        get => GetDataAtPosition(position);
        set => TryAddData(value, position, out _, true);
    }

    public T this[int x, int y, int z]
    {
        get => GetDataAtPosition(new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z));
        set => TryAddData(value, new MultidimensionalPosition((ushort)x, (ushort)y, (ushort)z), out _, true);
    }

    private static NTreeNode CreateNode(MultidimensionalPosition position, int id = -1, byte depth = 0)
    {
        if (nodePool.Count > 0)
        {
            return nodePool.Dequeue().Initialize(position, id, depth);
        }
        else
        {
            return new NTreeNode(position, id, depth);
        }
    }

    private static void ReleaseNode(NTreeNode node)
    {
        if(node.parent != null)
        {
            node.parent.RemoveChild(node);
        }
        nodePool.Enqueue(node.Reset());
        //Debug.Log($"Node Pool: {nodePool.Count}");
    }

    private NTreeNode GetNodeAtPosition(MultidimensionalPosition position, byte targetDepth = 0)
    {
        if (root == null || position.Depth > root.depth)
        {
            return null;
        }
        return root.GetLeaf(position, targetDepth);
    }

    public T GetDataAtPosition(MultidimensionalPosition position, byte targetDepth = 0)
    {
        NTreeNode node = GetNodeAtPosition(position, targetDepth);
        if (node == null) return default;
        return data[node.id];
    }

    public T GetDataFromIndex(ushort index)
    {
        return data[index];
    }

    public bool TryAddData(T data, MultidimensionalPosition position, out int dataIndex, bool overrideAtPosition = false)
    {
        if(root != null && GetNodeAtPosition(position) != null)
        {
            if (overrideAtPosition)
            {
                dataIndex = this.data.Count;
                this.data.Add(data);
                GetNodeAtPosition(position).id = dataIndex;
                return true;
            }
            else
            {
                dataIndex = 0;
                return false;
            }
        }
        if (highestDimensions < position.Dimensions)
        {
            highestDimensions = position.Dimensions;
        }
        dataIndex = this.data.Count;
        NTreeNode node = CreateNode(position, dataIndex);
        this.data.Add(data);
        nodes.Add(node);
        TryMoveNode(nodes.Count-1, position, overrideAtPosition);
        return true;
    }

    public bool TryMoveNode(int index, MultidimensionalPosition position, bool overrideAtPosition = false)
    {
        //Profiler.BeginSample("TryMoveNode");
        if (root != null && !overrideAtPosition && GetNodeAtPosition(position) != null) return false;
        /*for(int axis = 0; axis < position.dimensions; axis++)
        {
            if (position[axis] > ) TODO
        }*/
        NTreeNode leaf = nodes[index];
        //Debug.Log($"({positions[0]},{positions[1]},{positions[2]})");
        // Higher Depth Root
        depth = position.GetDepth(depth);
        if(root == null)
        {
            root = CreateNode(position, -1, (byte)(depth + 1));
            //Debug.Log($"New Root {root}(D{root.depth}, L{root.leafCount})");
        }
        else if(root.depth <= depth)
        {
            // Old root that is at a lower depth than the new one? Attach it with a new branch
            NTreeNode oldRoot = root;
            //Debug.Log($"Old Root {root}(D{root.depth}, L{root.leafCount})");
            root = CreateNode(position, -1, (byte)(depth + 1));
            Profiler.BeginSample("AddSector");
            root.AddSector(oldRoot, position);
            Profiler.EndSample();
            //Debug.Log($"New Root {root}(D{root.depth}, L{root.leafCount})");
        }

        if(leaf.parent == null)
        {
            // New Leaf
            // Recursively traverse from the root and add down to the new leaf
            Profiler.BeginSample("AddSector");
            root.AddSector(leaf, position);
            Profiler.EndSample();
        }
        else
        {
            // Existing Leaf
            // If this leaf already exists, rearange the branch to match the new path, breaking it off into a new branch if it reaches a shared node
            //Debug.Log($"Updating {leaf.GetPosition(3)} to {position}");
            Profiler.BeginSample("RearrangeBranch");
            RearrangeBranch(leaf, position);
            Profiler.EndSample();
            //Debug.Log($"New Position Actual: {leaf.GetPosition(3)}");
        }
        //Profiler.EndSample();
        return true;
    }

    public void Remove(MultidimensionalPosition position)
    {
        RemoveLeaf(root, position);
    }

    public void Remove(ushort index)
    {
        RemoveLeaf(root, nodes[index].position);
    }

    private void RemoveLeaf(NTreeNode node, MultidimensionalPosition position)
    {
        NTreeNode child = node.children[position.GetIndexAtDepth(depth - 1)];
        if (child != null) RemoveLeaf(child, position);
        if (node.childCount == 0)
        {
            ReleaseNode(node);
        }
    }

    /// <summary>
    /// Rearranges a branch's single-child nodes to match a new path, then returns the breakoff node to be reattached
    /// </summary>
    /// <param name="leaf">The leaf to start at</param>
    /// <param name="position">The new position</param>
    private void RearrangeBranch(NTreeNode leaf, MultidimensionalPosition position)
    {
        if (leaf.parent == null) return;

        Profiler.BeginSample("AddSector");
        leaf.UpdatePosition(position);
        Profiler.EndSample();
        if (leaf.parent.childCount > 1)
        {
            // We have reached the end of the path upwards, disconnect it and merge it back into the tree at its new position
            leaf.parent.RemoveChild(leaf);
            Profiler.BeginSample("AddSector");
            root.AddSector(leaf, position);
            Profiler.EndSample();
        }
        else
        {
            Profiler.BeginSample("RearrangeBranch Recurse");
            RearrangeBranch(leaf.parent, position);
            Profiler.EndSample();
        }
    }

    public T[] GetNodesInRadius(MultidimensionalPosition position, byte dimensionMask, int radius, Action<T, T, int[]> calculate)
    {
        List<NTreeNode> nearby = new List<NTreeNode>();
        NTreeNode leaf = GetNodeAtPosition(position);
        if (leaf == null || leaf.parent == null) return new T[0];

        MultidimensionalPosition startPosition = leaf.position;
        startPosition.MaskDimensions(dimensionMask);
        leaf.parent.AscendSearch(nearby, startPosition, radius, dimensionMask, leaf.IndexInParent);

        return nearby.Select(x => data[x.id]).ToArray();
    }

    public T[] GetNodesInRadius(ushort index, int radius, byte dimensionMask = 255, Action<T, T, int[]> calculate = null)
    {
        List<NTreeNode> nearby = new List<NTreeNode>();
        NTreeNode leaf = nodes[index];
        if (leaf == null || leaf.parent == null) return new T[0];

        MultidimensionalPosition startPosition = leaf.position;
        startPosition.MaskDimensions(dimensionMask);
        leaf.parent.AscendSearch(nearby, startPosition, radius, dimensionMask, leaf.IndexInParent);
        
        return nearby.Select(x => data[x.id]).ToArray();
    }
}

// Instead of calculate distance for every node pair for constraints, get the relative vector (and the inverse for the other node), then add them all together and calculate distance once. Use these sums for all constraints.
// This is two vector addition subtractions and one multiply per every node pair, and one distance calculation per node, very efficient!
// Try to keep the simulation below 250 million computations per second! With 8 core multithreading using 50% of the CPU, this should run the simulation at a smooth 60fps! 
// YOU CAN GET THE DISTANCE DURING THE RADIAL SEARCH!

