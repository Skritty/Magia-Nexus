using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component that will draw a given NTree using debug gizmos
/// </summary>
/*public class NTreeDebugRenderer<NodeDataType> : MonoBehaviour where NodeDataType : SimNodeData, new()
{
    public NTreeGenerator<NodeDataType> treeSource;
    private NTree<NodeDataType> tree => treeSource.Tree;
    public float nodesPerUnityUnit = 100;
    public bool setOptimization;
    public bool applySettings;

    private void OnValidate()
    {
        if (applySettings)
        {
            applySettings = false;
            if (setOptimization)
                UnityEditor.Compilation.CompilationPipeline.codeOptimization = UnityEditor.Compilation.CodeOptimization.Release;
            else
                UnityEditor.Compilation.CompilationPipeline.codeOptimization = UnityEditor.Compilation.CodeOptimization.Debug;
        }
    }

    private void OnDrawGizmos()
    {
        if (treeSource == null || tree == null) return;
        Vector3 pos;
        Vector3 center = Vector3.zero;
        MultidimensionalPosition position1;

        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(treeSource.spawnRadius, treeSource.spawnRadius, treeSource.spawnRadius) * 2 / nodesPerUnityUnit);

        foreach (NTree<NodeDataType>.NTreeNode node in tree.nodes)
        {
            position1 = node.position;
            Gizmos.color = Color.HSVToRGB((node.position[3] * 1f / (6 / Time.fixedDeltaTime)) % 1, .8f, 1f);
            pos = new Vector3(position1[0], position1[1], position1[2]) - Vector3.one * treeSource.spawnRadius;
            center += pos / nodesPerUnityUnit;
            //pos = new Vector3((position1[0] + node.Key.prePos[0]) / 2, (position1[1] + node.Key.prePos[1]) / 2, (position1[2] + node.Key.prePos[2]) / 2);
            Gizmos.DrawSphere(pos / nodesPerUnityUnit, 0.1f);// * node.Key.nearby / nodeCount);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center / tree.nodes.Count, 0.1f);
    }
}
*/