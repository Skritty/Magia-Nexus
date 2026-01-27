using UnityEngine;

[System.Serializable]
public class NTreeGenerator<NodeDataType> : MonoBehaviour where NodeDataType : new()
{
    public NTree<NodeDataType> Tree => tree;
    private NTree<NodeDataType> tree;

    public int nodeCount = 100;
    public int spawnRadius = 200;

    public void GenerateTreeSpherical()
    {
        tree = new NTree<NodeDataType>();
        for (int i = 0; i < nodeCount; i++)
        {
            float theta = Random.Range(0f, Mathf.PI);
            float phi = Random.Range(0f, 2 * Mathf.PI);
            float radius = spawnRadius * Random.Range(0, 1f);// Mathf.Sin(Random.Range(0, Mathf.PI * 0.5f));
            float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
            float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
            float z = radius * Mathf.Cos(theta);
            MultidimensionalPosition position = new MultidimensionalPosition((ushort)(x + spawnRadius), (ushort)(z + spawnRadius), (ushort)(y + spawnRadius), (ushort)i);
            NodeDataType node = new NodeDataType();
            tree.TryAddData(node, position, out _);
        }
    }
}