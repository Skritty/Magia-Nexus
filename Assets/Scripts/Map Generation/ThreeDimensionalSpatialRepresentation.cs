using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ThreeDimensionalSpatialRepresentation<T> : IEnumerable, ISerializationCallbackReceiver
{
    public T[,,] objects;
    [HideInInspector]
    public int x, y, z;
    [SerializeField, HideInInspector]
    private List<SerializedSpatialObject> serializedObjects = new();
    [Serializable]
    public struct SerializedSpatialObject
    {
        public T obj;
        public int x, y, z;

        public SerializedSpatialObject(T obj, int x, int y, int z)
        {
            this.obj = obj;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public ThreeDimensionalSpatialRepresentation(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        objects = new T[x, y, z];
    }

    public IEnumerator GetEnumerator() => objects?.GetEnumerator();

    public T this[int x, int y, int z]
    {
        get => objects[x, y, z];
        set => objects[x, y, z] = value;
    }

    public T this[(int,int,int) index]
    {
        get => objects[index.Item1, index.Item2, index.Item3];
        set => objects[index.Item1, index.Item2, index.Item3] = value;
    }

    public void OnAfterDeserialize()
    {
        if (serializedObjects == null || serializedObjects.Count == 0) return;
        objects = new T[x, y, z];
        foreach(SerializedSpatialObject serializedObj in serializedObjects)
        {
            objects[serializedObj.x, serializedObj.y, serializedObj.z] = serializedObj.obj;
        }
    }

    public void OnBeforeSerialize()
    {
        if (objects == null) return;
        serializedObjects.Clear();
        for (int x = 0; x < this.x; x++)
        {
            for (int y = 0; y < this.y; y++)
            {
                for (int z = 0; z < this.z; z++)
                {
                    serializedObjects.Add(new SerializedSpatialObject(objects[x,y,z], x, y, z));
                }
            }
        }
    }

    public (int,int,int) GetIndex(T obj)
    {
        for (int x = 0; x < this.x; x++)
        {
            for (int y = 0; y < this.y; y++)
            {
                for (int z = 0; z < this.z; z++)
                {
                    if (objects[x, y, z].Equals(obj))
                    {
                        return (x, y, z);
                    }
                }
            }
        }
        return (int.MinValue, int.MinValue, int.MinValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>An array of size 6, which can contain null. Order of adjecency is x-y-z, (-1)->(+1).</returns>
    public T[] GetAdjecentObjects(T obj)
    {
        T[] adjecent = new T[6];
        (int,int,int) index = GetIndex(obj);
        if (index.Item1 < 0) return adjecent;
        return GetAdjecentObjects(index);
    }

    public T[] GetAdjecentObjects((int,int,int) index)
    {
        T[] adjecent = new T[6];
        if ((uint)(index.Item1 + 1) < x && (uint)index.Item2 < y && (uint)index.Item3 < z) adjecent[0] = objects[index.Item1 + 1, index.Item2, index.Item3];
        if ((uint)(index.Item1 - 1) < x && (uint)index.Item2 < y && (uint)index.Item3 < z) adjecent[1] = objects[index.Item1 - 1, index.Item2, index.Item3];
        if ((uint)index.Item1 < x && (uint)(index.Item2 + 1) < y && (uint)index.Item3 < z) adjecent[2] = objects[index.Item1, index.Item2 + 1, index.Item3];
        if ((uint)index.Item1 < x && (uint)(index.Item2 - 1) < y && (uint)index.Item3 < z) adjecent[3] = objects[index.Item1, index.Item2 - 1, index.Item3];
        if ((uint)index.Item1 < x && (uint)index.Item2 < y && (uint)(index.Item3 + 1) < z) adjecent[4] = objects[index.Item1, index.Item2, index.Item3 + 1];
        if ((uint)index.Item1 < x && (uint)index.Item2 < y && (uint)(index.Item3 - 1) < z) adjecent[5] = objects[index.Item1, index.Item2, index.Item3 - 1];
        return adjecent;
    }
}

// 4x3x2
// 0, 1, 2, 3       // 12, 13, 14, 15
// 4, 5, 6, 7       // 16, 17, 18, 19 
// 8, 9, 10, 11    // 20, 21, 22, 23