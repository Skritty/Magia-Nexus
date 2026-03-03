using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An array of ushorts of indeterminate length, with extra functionality
/// </summary>
[Serializable]
public class MultidimensionalPosition : IEnumerable, IEqualityComparer<MultidimensionalPosition>
{
    [SerializeField]
    private ushort[] position;

    public MultidimensionalPosition(int dimensions)
    {
        this.position = new ushort[dimensions];
    }
    public MultidimensionalPosition(params ushort[] position)
    {
        this.position = position;
    }

    public int Dimensions => position.Length;
    public int Depth
    {
        get
        {
            int highestBit = 0;
            for (int axis = 0; axis < Dimensions; axis++)
            {
                for(int i = highestBit + 1; i < 16; i++)
                {
                    if ((position[axis] & (1 << i)) != 0)
                    {
                        highestBit = i;
                    }
                }
            }
            return highestBit;
        }
    }
    public Vector3 ToVector3
    {
        get
        {
            Vector3 vector = new Vector3();
            for (int axis = 0; axis < Dimensions; axis++)
            {
                if (axis == 3) break;
                vector[axis] = position[axis];
            }
            return vector;
        }
    }

    public ushort this[int key]
    {
        get => position[key];
        set => position[key] = value;
    }

    public static MultidimensionalPosition operator +(MultidimensionalPosition operand, MultidimensionalPosition other)
    {
        MultidimensionalPosition newPosition = operand;
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] += other[axis];
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator -(MultidimensionalPosition operand, MultidimensionalPosition other)
    {
        MultidimensionalPosition newPosition = operand;
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            if (operand[axis] >= other[axis])
            {
                newPosition[axis] -= other[axis];
            }
            else
            {
                newPosition[axis] = (ushort)(other[axis] - newPosition[axis]);
            }
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator &(MultidimensionalPosition operand, int mask)
    {
        MultidimensionalPosition newPosition = operand.Clone();
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] &= (ushort)mask;
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator |(MultidimensionalPosition operand, int mask)
    {
        MultidimensionalPosition newPosition = operand.Clone();
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] |= (ushort)mask;
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator ~(MultidimensionalPosition operand)
    {
        MultidimensionalPosition newPosition = operand.Clone();
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] = (ushort)~operand[axis];
        }
        return newPosition;
    }

    public int GetIndexAtDepth(int depth)
    {
        ushort index = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if (0 != (position[axis] & 1 << depth)) index = (ushort)(index | 1 << axis);
        }
        return index;
    }

    public void MaskDimensions(byte dimensionMask)
    {
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if (0 == (dimensionMask & 1 << axis))
            {
                position[axis] = 0;
            }
        }
    }

    /// <summary>
    /// Returns a new position ignoring bit depth at or below the passed in depth.
    /// </summary>
    public MultidimensionalPosition PositionAtDepth(int depth)
    {
        MultidimensionalPosition newPosition = new MultidimensionalPosition();
        for (int axis = 0; axis < Dimensions; axis++)
        {
            newPosition[axis] = (ushort)(position[axis] & (~0 << depth));
        }
        return newPosition;
    }

    public MultidimensionalPosition FarthestPositionAtDepth(int depth)
    {
        MultidimensionalPosition farthest = this;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if (0 == (position[axis] & 1 << depth))
            {
                farthest.position[axis] &= (ushort)(~0 << depth);
            }
            else
            {
                farthest.position[axis] |= (ushort)~(~0 << depth);
            }
        }
        return farthest;
    }

    public MultidimensionalPosition ClosestPositionAtDepth(int depth)
    {
        MultidimensionalPosition farthest = this;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if (0 != (position[axis] & 1 << depth))
            {
                farthest.position[axis] &= (ushort)(~0 << depth);
            }
            else
            {
                farthest.position[axis] |= (ushort)~(~0 << depth);
            }
        }
        return farthest;
    }

    public bool[] GetBitsAtDepth(int depth)
    {
        bool[] bits = new bool[Dimensions];
        for (int axis = 0; axis < Dimensions; axis++)
        {
            bits[axis] = 0 != (position[axis] & 1 << depth);
        }
        return bits;
    }

    // TODO: Make dist more accurate
    public int GetDistApprox(MultidimensionalPosition other)
    {
        int dist = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if(position[axis] >= other[axis])
            {
                dist += position[axis] - other[axis];
            }
            else
            {
                dist += other[axis] - position[axis];
            }
        }
        return dist;
    }

    public float Distance(MultidimensionalPosition other)
    {
        float dist = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if(position[axis] >= other[axis])
            {
                dist += Mathf.Pow((position[axis] - other[axis]), 2);
            }
            else
            {
                dist += Mathf.Pow((other[axis] - position[axis]), 2);
            }
        }
        return Mathf.Sqrt(dist);
    }

    public bool EqualsFromDepthUp(MultidimensionalPosition compareTo, byte depth)
    {
        for (int axis = 0; axis < position.Length; axis++)
        {
            if ((position[axis] >> depth << depth) != (compareTo[axis] >> depth << depth))
            {
                return false;
            }
        }
        return true;
    }

    public byte GetDepth(byte depth = 0)
    {
        for (int axis = 0; axis < position.Length; axis++)
        {
            while ((1 << (depth + 1)) <= position[axis])
            {
                depth++;
            }
        }
        return depth;
    }

    public override string ToString()
    {
        return "("+ string.Join(',', position) + ')';
    }

    public IEnumerator GetEnumerator()
    {
        return position.GetEnumerator();
    }

    public MultidimensionalPosition Clone()
    {
        MultidimensionalPosition pos = new(Dimensions);
        for(int axis = 0; axis < Dimensions; axis++)
        {
            pos[axis] = position[axis];
        }
        return pos;
    }
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(MultidimensionalPosition)) return false;
        return Equals(this, obj as MultidimensionalPosition);
    }

    public bool Equals(MultidimensionalPosition x, MultidimensionalPosition y)
    {
        int highestDimension = x.Dimensions > y.Dimensions ? x.Dimensions : y.Dimensions;
        for (int axis = 0; axis < highestDimension; axis++)
        {
            /*if (x.dimensions < axis)
            {
                if (0 != y[axis]) return false;
            }
            else if (y.dimensions < axis)
            {
                if (x[axis] != 0) return false;
            }
            else */if (x[axis] != y[axis]) return false;
        }
        return true;
    }

    public int GetHashCode(MultidimensionalPosition obj)
    {
        int hash = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            hash = (hash*397)^position[axis];
        }
        return hash;
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }
}