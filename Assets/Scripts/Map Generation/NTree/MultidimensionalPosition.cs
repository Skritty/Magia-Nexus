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
    private int[] position;

    public MultidimensionalPosition(int dimensions)
    {
        position = new int[dimensions];
    }
    public MultidimensionalPosition(params int[] position)
    {
        this.position = position;
    }
    public MultidimensionalPosition(MultidimensionalPosition other)
    {
        position = new int[other.Dimensions];
        for (int axis = 0; axis < Dimensions; axis++)
        {
            position[axis] = other[axis];
        }
    }

    public int Dimensions => position.Length;
    public Vector3 ToVector3
    {
        get
        {
            Vector3 vector = new Vector3();
            for (int axis = 0; axis < Dimensions; axis++)
            {
                vector[axis] = position[axis];
                if (axis == 2) break;
            }
            return vector;
        }
    }

    public int this[int key]
    {
        get => position[key];
        set => position[key] = value;
    }

    public static MultidimensionalPosition operator +(MultidimensionalPosition operand, MultidimensionalPosition other)
    {
        MultidimensionalPosition newPosition = new(operand);
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] += other[axis];
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator -(MultidimensionalPosition operand, MultidimensionalPosition other)
    {
        MultidimensionalPosition newPosition = new(operand);
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] -= other[axis];
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator &(MultidimensionalPosition operand, int mask)
    {
        MultidimensionalPosition newPosition = new(operand);
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] &= mask;
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator |(MultidimensionalPosition operand, int mask)
    {
        MultidimensionalPosition newPosition = new(operand);
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] |= mask;
        }
        return newPosition;
    }

    public static MultidimensionalPosition operator ~(MultidimensionalPosition operand)
    {
        MultidimensionalPosition newPosition = new(operand);
        for (int axis = 0; axis < newPosition.Dimensions; axis++)
        {
            newPosition[axis] = ~operand[axis];
        }
        return newPosition;
    }

    public int GetIndexAtDepth(int depth)
    {
        int index = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if ((position[axis] & 1 << depth) != 0) index |= 1 << axis;
        }
        return index;
    }

    public void MaskDimensions(byte dimensionMask)
    {
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if ((dimensionMask & 1 << axis) == 0)
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
        MultidimensionalPosition newPosition = new(this);
        for (int axis = 0; axis < Dimensions; axis++)
        {
            newPosition[axis] = position[axis] & (~0 << depth);
        }
        return newPosition;
    }

    public MultidimensionalPosition FarthestPositionAtDepth(int depth)
    {
        MultidimensionalPosition farthest = new(this);
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if ((position[axis] & 1 << depth) == 0)
            {
                farthest.position[axis] &= ~0 << depth;
            }
            else
            {
                farthest.position[axis] |= ~(~0 << depth);
            }
        }
        return farthest;
    }

    public MultidimensionalPosition ClosestPositionAtDepth(int depth)
    {
        MultidimensionalPosition closest = new(this);
        for (int axis = 0; axis < Dimensions; axis++)
        {
            if ((position[axis] & 1 << depth) != 0)
            {
                closest.position[axis] &= ~0 << depth;
            }
            else
            {
                closest.position[axis] |= ~(~0 << depth);
            }
        }
        return closest;
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
            dist += Mathf.Pow(position[axis] - other[axis], 2);
        }
        return Mathf.Sqrt(dist);
    }

    public bool EqualsFromDepthUp(MultidimensionalPosition other, byte depth)
    {
        for (int axis = 0; axis < position.Length; axis++)
        {
            if ((position[axis] >> depth << depth) != (other[axis] >> depth << depth))
            {
                return false;
            }
        }
        return true;
    }

    public byte GetDepth()
    {
        byte highestBit = 0;
        for (int axis = 0; axis < Dimensions; axis++)
        {
            for (byte i = 30; i > highestBit; i--) // Ignore the sign bit (bit position 31)
            {
                if ((position[axis] < 0 && (position[axis] & (1 << i)) == 0) // For negative numbers, the value is lower for a 0 bit
                    || ((position[axis] >= 0 && (position[axis] & (1 << i)) != 0)))
                {
                    highestBit = i;
                    break;
                }
            }
        }
        return highestBit;
    }

    /*public byte GetDepth(byte depth = 0) TODO: this version is probably more efficient, make it work with negatives
    {
        for (int axis = 0; axis < position.Length; axis++)
        {
            while (position[axis] >= (1 << depth))
            {
                depth++;
            }
        }
        return depth;
    }*/

    public override string ToString()
    {
        return "("+ string.Join(',', position) + ')';
    }

    public IEnumerator GetEnumerator()
    {
        return position.GetEnumerator();
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(MultidimensionalPosition)) return false;
        return Equals(this, obj as MultidimensionalPosition);
    }

    public bool Equals(MultidimensionalPosition pos1, MultidimensionalPosition pos2)
    {
        int highestDimension = pos1.Dimensions > pos2.Dimensions ? pos1.Dimensions : pos2.Dimensions;
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
            else */if (pos1[axis] != pos2[axis]) return false;
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