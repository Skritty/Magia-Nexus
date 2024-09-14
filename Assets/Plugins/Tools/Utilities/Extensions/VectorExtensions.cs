//Created by: Julian Noel
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    public static class VectorExtensions
    {
        #region Inverse Vectors
        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(1 / v.x, 1 / v.y);
        }

        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
        }

        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector4 Inverse(this Vector4 v)
        {
            return new Vector4(1 / v.x, 1 / v.y, 1 / v.z, 1 / v.w);
        }
        #endregion Inverse Vectors

        #region MaxComponent

        public static float MaxComponent(this Vector2 vec)
        {
            return Mathf.Max(vec.x, vec.y);
        }

        public static float MaxComponent(this Vector3 vec)
        {
            return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
        }

        public static float MaxComponent(this Vector4 vec)
        {
            return Mathf.Max(Mathf.Max(vec.x, vec.y), Mathf.Max(vec.z, vec.w));
        }

        #endregion MaxComponent

        #region MinComponent

        public static float MinComponent(this Vector2 vec)
        {
            return Mathf.Min(vec.x, vec.y);
        }

        public static float MinComponent(this Vector3 vec)
        {
            return Mathf.Min(vec.x, vec.y, vec.z);
        }

        public static float MinComponent(this Vector4 vec)
        {
            return Mathf.Min(vec.x, vec.y, vec.z, vec.w);
        }

        #endregion MinComponent

        #region Average

        public static Vector3 Average(params Vector3[] vectors)
        {
            Vector3 sum = Vector3.zero;

            for(int i = 0; i < vectors.Length; i++)
            {
                sum += vectors[i];
            }

            return sum / vectors.Length;
        }

        #endregion Average

        public static Vector3 ScaleToMagnitude(this Vector3 vec, float newMagnitude)
        {
            return vec.normalized * newMagnitude;
        }
        /// <summary> abs(x * y)</summary>
        public static float UnsignedComponentProduct(this Vector2 vec) => Mathf.Abs(vec.x * vec.y);

        /// <summary> abs(x * y * z)</summary>
        public static float UnsignedComponentProduct(this Vector3 vec) => Mathf.Abs(vec.x * vec.y * vec.z);

        /// <summary> abs(x * y * z * w)</summary>
        public static float UnsignedComponentProduct(this Vector4 vec) => Mathf.Abs(vec.x * vec.y * vec.z * vec.w);

        /// <summary> x * y </summary>
        public static float SignedComponentProduct(this Vector2 vec) => vec.x * vec.y;

        /// <summary> x * y * z </summary>
        public static float SignedComponentProduct(this Vector3 vec) => vec.x * vec.y * vec.z;

        /// <summary> x * y * z * w</summary>
        public static float SignedComponentProduct(this Vector4 vec) => vec.x * vec.y * vec.z * vec.w;

        /// <summary>
        /// The SIGNED area of a parallelogram/rectangle with the this vector's width/height
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedArea(this Vector2 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED volume of a cuboid with the this vector's width/height/depth
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedVolume(this Vector3 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED measure/4-volume/"hypervolume" of a 4-cuboid with the this vector's scale
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedHypervolume(this Vector4 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED area of a parallelogram/rectangle with the this vector's width/height
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedArea(this Vector2 vec) => UnsignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED volume of a cuboid with the this vector's width/height/depth
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedVolume(this Vector3 vec) => UnsignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED measure/4-volume/"hypervolume" of a 4-cuboid with the this vector's scale
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedHyperVolume(this Vector4 vec) => UnsignedComponentProduct(vec);
    }
}
