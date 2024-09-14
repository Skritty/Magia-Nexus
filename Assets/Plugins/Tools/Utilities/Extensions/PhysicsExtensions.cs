//Edited by: Julian Noel

// MIT License
// 
// Copyright (c) 2017 Justin Larrabee <justonia@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skritty.Tools.Utilities
{
    public static class PhysicsExtensions
    {
        //
        // Box
        //

        #region BoxCast
        public static bool BoxCast(BoxCollider box, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool BoxCast(BoxCollider box, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static RaycastHit[] BoxCastAll(BoxCollider box, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static int BoxCastNonAlloc(BoxCollider box, Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask, queryTriggerInteraction);
        }
        #endregion

        #region Check and Overlap Box

        /// <summary>
        /// BoxCollider overload for Physics.CheckBox()
        /// </summary>
        /// <param name="box"></param>
        /// <param name="layerMask"></param>
        /// <param name="queryTriggerInteraction"></param>
        /// <returns></returns>
        public static bool CheckBox(BoxCollider box, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.CheckBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
        }

        public static Collider[] OverlapBox(BoxCollider box, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.OverlapBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
        }

        public static int OverlapBoxNonAlloc(BoxCollider box, Collider[] results, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center, halfExtents;
            Quaternion orientation;
            box.ToWorldSpaceBox(out center, out halfExtents, out orientation);
            return Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// Determines of the given boxCollider is fully inside the other.
        /// </summary>
        /// <remarks>
        /// Does so by first checking the bounds for a short-circuit opportunity 
        /// and then checks each corner point of the box. Short circuits when a point 
        /// is found outside of it.
        /// </remarks>
        /// <param name="thisBox"></param>
        /// <param name="other"></param>
        /// <returns>True if fully containd, false otherwise.</returns>
        public static bool Contains(this BoxCollider thisBox, BoxCollider other)
        {
            //Bounds intersect short circuit test
            if(thisBox.bounds.Intersects(other.bounds))
            {
                //determine if all the corners of other are contained within the transformed bounds of thisBox.
                //Need halves since we're measuring from the center.
                float halfWidth = other.size.x / 2;
                float halfHeight = other.size.y / 2;
                float halfDepth = other.size.z / 2;

                //a BoxCollider has 8 corners
                Vector3[] boxSpaceCorners = {
                    new(halfWidth, halfHeight, halfDepth),
                    new(halfWidth, halfHeight, -halfDepth),
                    new(halfWidth, -halfHeight, halfDepth),
                    new(halfWidth, -halfHeight, -halfDepth),
                    new(-halfWidth, halfHeight, halfDepth),
                    new(-halfWidth, halfHeight, -halfDepth),
                    new(-halfWidth, -halfHeight, halfDepth),
                    new(-halfWidth, -halfHeight, -halfDepth),
                };

                foreach(Vector3 corner in boxSpaceCorners)
                {
                    //map otherLocal => world before plugging in
                    Vector3 localSpaceCorner = corner + other.center;
                    Vector3 worldSpaceCorner = other.transform.TransformPoint(localSpaceCorner);

                    //short circuit and exit if the corner is not contained
                    if(!thisBox.Contains(worldSpaceCorner))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Determines whether the given point is within this BoxCollider
        /// </summary>
        /// <param name="thisBox"></param>
        /// <param name="point"></param>
        /// <param name="isWorldSpace"></param>
        /// <returns>True if fully contained, false otherwise. Border is inclusive.</returns>
        public static bool Contains(this BoxCollider thisBox, Vector3 point, bool isWorldSpace = true)
        {
            if(thisBox.bounds.Contains(point))
            {
                //The point transformed into the local space of thisBox
                Vector3 rawLocalPoint = isWorldSpace ? thisBox.transform.InverseTransformPoint(point) : point;
                //adjust to collider position
                Vector3 offsetLocalPoint = rawLocalPoint - thisBox.center;

                if(Mathf.Abs(offsetLocalPoint.x) <= thisBox.size.x &&
                    Mathf.Abs(offsetLocalPoint.y) <= thisBox.size.y &&
                    Mathf.Abs(offsetLocalPoint.z) <= thisBox.size.z)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public static void ToWorldSpaceBox(this BoxCollider box, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
        {
            orientation = box.transform.rotation;
            center = box.transform.TransformPoint(box.center);
            Vector3 lossyScale = box.transform.lossyScale;
            Vector3 scale = AbsVec3(lossyScale);
            halfExtents = Vector3.Scale(scale, box.size) * 0.5f;
        }

        #region Random

        public static Vector3 GetRandomPointInside(this BoxCollider box, bool useLocalSpace = false)
        {
            Vector3 randomLocal = new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f);
            Vector3 localPosition = Vector3.Scale(randomLocal, box.size) + box.center;

            return useLocalSpace ? localPosition : box.transform.TransformPoint(localPosition);
        }

        public static Vector3 GetRandomPointOnSurface(this BoxCollider box, bool useLocalSpace = false)
        {
            //picking a random face on a 6-sided shape
            int randomFaceIndex = UnityEngine.Random.Range(0, 6);

            //Pick a random point on the respective face
            Vector3 randomLocal;
            float randX = UnityEngine.Random.value - 0.5f;
            float randY = UnityEngine.Random.value - 0.5f;

            switch(randomFaceIndex)
            {
                case 0:
                    randomLocal = new Vector3(-1f, randX, randY);
                    break;
                case 1:
                    randomLocal = new Vector3(1f, randX, randY);
                    break;
                case 2:
                    randomLocal = new Vector3(randX, -1f, randY);
                    break;
                case 3:
                    randomLocal = new Vector3(randX, 1f, randY);
                    break;
                case 4:
                    randomLocal = new Vector3(randX, randY, -1f);
                    break;
                case 5:
                    randomLocal = new Vector3(randX, randY, 1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Not sure how you got here from UnityEngine.Random.Range(0, 6)...");
            }

            Vector3 localPosition = Vector3.Scale(randomLocal, box.size) + box.center;

            return useLocalSpace ? localPosition : box.transform.TransformPoint(localPosition);
        }

        #endregion

        //
        // Sphere
        //

        #region SphereCast
        public static bool SphereCast(SphereCollider sphere, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.SphereCast(center, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(SphereCollider sphere, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.SphereCastAll(center, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(SphereCollider sphere, Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.SphereCastNonAlloc(center, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region Check and Overlap Sphere

        public static bool CheckSphere(SphereCollider sphere, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.CheckSphere(center, radius, layerMask, queryTriggerInteraction);
        }

        public static Collider[] OverlapSphere(SphereCollider sphere, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.OverlapSphere(center, radius, layerMask, queryTriggerInteraction);
        }

        public static int OverlapSphereNonAlloc(SphereCollider sphere, Collider[] results, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 center;
            float radius;
            sphere.ToWorldSpaceSphere(out center, out radius);
            return Physics.OverlapSphereNonAlloc(center, radius, results, layerMask, queryTriggerInteraction);
        }

        #endregion

        public static void ToWorldSpaceSphere(this SphereCollider sphere, out Vector3 center, out float radius)
        {
            center = sphere.transform.TransformPoint(sphere.center);
            radius = sphere.radius * MaxVec3(AbsVec3(sphere.transform.lossyScale));
        }

        #region Random

        public static Vector3 GetRandomPointInside(this SphereCollider sphere, bool useLocalSpace = false)
        {
            Vector3 localPosition = (UnityEngine.Random.insideUnitSphere * sphere.radius) + sphere.center;
            return useLocalSpace ? localPosition : sphere.transform.TransformPoint(localPosition);
        }

        public static Vector3 GetRandomPointOnSurface(this SphereCollider sphere, bool useLocalSpace = false)
        {
            Vector3 localPosition = (UnityEngine.Random.onUnitSphere * sphere.radius) + sphere.center;
            return useLocalSpace ? localPosition : sphere.transform.TransformPoint(localPosition);
        }

        #endregion

        //
        // Capsule
        //

        #region CapsuleCast

        public static bool CapsuleCast(CapsuleCollider capsule, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.CapsuleCast(point0, point1, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static RaycastHit[] CapsuleCastAll(CapsuleCollider capsule, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.CapsuleCastAll(point0, point1, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static int CapsuleCastNonAlloc(CapsuleCollider capsule, Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.CapsuleCastNonAlloc(point0, point1, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region Check and Overlap Capsule

        public static bool CheckCapsule(CapsuleCollider capsule, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.CheckCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
        }

        public static Collider[] OverlapCapsule(CapsuleCollider capsule, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.OverlapCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
        }

        public static int OverlapCapsuleNonAlloc(CapsuleCollider capsule, Collider[] results, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 point0, point1;
            float radius;
            capsule.ToWorldSpaceCapsule(out point0, out point1, out radius);
            return Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, queryTriggerInteraction);
        }

        #endregion

        public static void ToWorldSpaceCapsule(this CapsuleCollider capsule, out Vector3 point0, out Vector3 point1, out float radius)
        {
            Vector3 center = capsule.transform.TransformPoint(capsule.center);
            radius = 0f;
            float height = 0f;
            Vector3 lossyScale = AbsVec3(capsule.transform.lossyScale);
            Vector3 dir = Vector3.zero;

            switch(capsule.direction)
            {
                case 0: // x
                    radius = Mathf.Max(lossyScale.y, lossyScale.z) * capsule.radius;
                    height = lossyScale.x * capsule.height;
                    dir = capsule.transform.TransformDirection(Vector3.right);
                    break;
                case 1: // y
                    radius = Mathf.Max(lossyScale.x, lossyScale.z) * capsule.radius;
                    height = lossyScale.y * capsule.height;
                    dir = capsule.transform.TransformDirection(Vector3.up);
                    break;
                case 2: // z
                    radius = Mathf.Max(lossyScale.x, lossyScale.y) * capsule.radius;
                    height = lossyScale.z * capsule.height;
                    dir = capsule.transform.TransformDirection(Vector3.forward);
                    break;
            }

            if(height < radius * 2f)
            {
                dir = Vector3.zero;
            }

            point0 = center + dir * (height * 0.5f - radius);
            point1 = center - dir * (height * 0.5f - radius);
        }

        //  
        // Util
        //

        public static void SortClosestToFurthest(RaycastHit[] hits, int hitCount = -1)
        {
            if(hitCount == 0)
            {
                return;
            }

            if(hitCount < 0)
            {
                hitCount = hits.Length;
            }

            Array.Sort<RaycastHit>(hits, 0, hitCount, ascendDistance);
        }

        /// <summary>
        /// Returns the smallest Bounds that encapsulates the Bounds of all the colliders in the enumerable.
        /// </summary>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static Bounds? GetNullableUnionBounds(this IEnumerable<Collider> cols)
        {
            #region Validation Checks
            if(cols == null)
            {
                ConsoleLog.LogError($"Given IEnumerable \"{nameof(cols)}\" cannot be null!");
                return null;
            }

            if(cols.Count() <= 0)
            {
                ConsoleLog.LogError($"Given IEnumerable \"{nameof(cols)}\" cannot be empty!");
                return null;
            }
            #endregion Validation Checks

            Bounds unionBounds = cols.First().bounds; //start with first entry; default includes the origin

            foreach(Collider col in cols)
            {
                unionBounds.Encapsulate(col.bounds);
            }

            return unionBounds;
        }

        /// <summary>
        /// Determines whether the other Bounds are fully encapsulated by this one.
        /// </summary>
        /// <remarks>
        /// Since <see cref="Bounds"/> are axis-aligned, they can be defined using either a center and half-extents OR 
        /// with their min and max coordinates (like a dragged selection box, but in 3D). 
        /// Determining if a <see cref="Bounds"/> is fully encapsulated by another is as simple as checking if 
        /// these 2 points are inside it.
        /// </remarks>
        /// <param name="outerBounds">The Bounds that may potentially contain innerBounds.</param>
        /// <param name="innerBounds">The Bounds potentially contained within outerBounds.</param>
        /// <returns>True if fully contained/encapsulated, false otherwise.</returns>
        public static bool Contains(this Bounds outerBounds, Bounds innerBounds)
        {
            return outerBounds.Contains(innerBounds.min) && outerBounds.Contains(innerBounds.max);
        }

        /// <summary>
        /// Gets volume of the AABB bounds formed by the intersection of a and b.
        /// Commutative operation.
        /// Returns 0 if the bounds don't actually intersect.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float GetIntersectionVolume(this Bounds a, Bounds b)
        {
            if(a.Contains(b))
            {
                return b.Volume();
            }
            else if(b.Contains(a))
            {
                return a.Volume();
            }
            else
            {
                return a.GetIntersectionWith(b).Volume();
            }
        }

        /// <summary>
        /// Returns the smaller bounds which covers the intersection of this bounds and another.
        /// If the two don't actually intersect, it return zero.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Bounds GetIntersectionWith(this Bounds a, Bounds b)
        {
            Bounds output = new();

            if(!a.Intersects(b)) return output;

            // The min and max points; Bounds calculates these each time they are reffed, so caching
            Vector3 minA = a.min;
            Vector3 maxA = a.max;
            Vector3 minB = b.min;
            Vector3 maxB = b.max;

            Vector3 min = new(
                Mathf.Max(minA.x, minB.x),
                Mathf.Max(minA.y, minB.y),
                Mathf.Max(minA.z, minB.z));

            Vector3 max = new(
                Mathf.Min(maxA.x, maxB.x),
                Mathf.Min(maxA.y, maxB.y),
                Mathf.Min(maxA.z, maxB.z));

            output.SetMinMax(min, max);
            return output;
        }

        //Nullable overload
        public static float PercentContainedIn(this Bounds? ourBounds, Bounds containerBounds)
        {
            return ourBounds != null ?
                PercentContainedIn(ourBounds.Value, containerBounds) : 0f;
        }

        //Nullable overload
        public static float PercentContainedIn(this Bounds ourBounds, Bounds? containerBounds)
        {
            return containerBounds != null ?
                PercentContainedIn(ourBounds, containerBounds.Value) : 0f;
        }

        //Nullable overload
        public static float PercentContainedIn(this Bounds? ourBounds, Bounds? containerBounds)
        {
            return ourBounds != null && containerBounds != null ? 
                PercentContainedIn(ourBounds.Value, containerBounds.Value) : 0f;
        }

        /// <summary>
        /// Determines what percentage of this bounds is contained within/intersecting with the other given bounds.
        /// If our bounds is half submerged into the over collider, this returns 0.5, 
        /// regardless of how big the other collider is (assuming it's still big enough to encapsulate 50% of this collider)
        /// </summary>
        /// <param name="ourBounds"></param>
        /// <param name="containerBounds"></param>
        /// <returns></returns>
        public static float PercentContainedIn(this Bounds ourBounds, Bounds containerBounds)
        {
            if(containerBounds.Contains(ourBounds))
            {
                //A lovely short circuit
                return 1f;
            }
            else
            {
                return ourBounds.GetIntersectionVolume(containerBounds) / ourBounds.Volume();
            }
        }

        public static float Volume(this Bounds bounds) => bounds.size.SignedComponentProduct();

        #region LayerMask Includes

        /// <summary>
        /// Determines if the given layer is included in this mask.
        /// </summary>
        /// <remarks>
        /// LayerMasks use bitmasks; Bitshift 1 by the layer number to position it as it would be in the mask.
        /// If the operation "mask OR shifted-layer" doesn't change the mask, it's because it was already included.
        /// </remarks>
        /// <param name="layerNumber">the layer number (not in bitmask form). 
        /// Can be obtained with LayerMask.NameToLayer().</param>
        /// <returns>True if contained/included in the mask, false otherwise.</returns>
        public static bool Includes(this LayerMask mask, int layerNumber)
        {
            //Bitshift the layer to position it as it would be in the mask
            //If a mask OR shifted-layer doesn't change the mask, it's because it was already included
            return (mask | (1 << layerNumber)) == mask;
        }

        /// <summary>
        /// Determines if the given layer is included in this mask.
        /// </summary>
        /// <remarks>
        /// LayerMasks use bitmasks; Bitshift 1 by the layer number to position it as it would be in the mask.
        /// If the operation "mask OR shifted-layer" doesn't change the mask, it's because it was already included.
        /// </remarks>
        /// <param name="layerNumber">the layer number (not in bitmask form). 
        /// Can be obtained with LayerMask.NameToLayer().</param>
        /// <returns>True if contained/included in the mask, false otherwise.</returns>
        public static bool Includes(this LayerMask mask, string layerName)
        {
            return mask.Includes(LayerMask.NameToLayer(layerName));
        }

        /// <summary>
        /// Determines if the given layer is included in this mask.
        /// </summary>
        /// <remarks>
        /// LayerMasks use bitmasks; Bitshift 1 by the layer number to position it as it would be in the mask.
        /// <br/>
        /// If the operation "mask OR shifted-layer" doesn't change the mask, it's because it was already included.
        /// </remarks>
        /// <param name="collider">The collider of the gameobject we're checking the layer of.</param>
        /// <returns>True if contained/included in the mask, false otherwise.</returns>
        public static bool Includes(this LayerMask mask, Collider collider)
        {
            return mask.Includes(collider.gameObject.layer);
        }

        /// <summary>
        /// Determines if the given layer is included in this mask.
        /// </summary>
        /// <remarks>
        /// LayerMasks use bitmasks; Bitshift 1 by the layer number to position it as it would be in the mask.
        /// <br/>
        /// If the operation "mask OR shifted-layer" doesn't change the mask, it's because it was already included.
        /// </remarks>
        /// <param name="collider">The collider of the gameobject we're checking the layer of.</param>
        /// <returns>True if contained/included in the mask, false otherwise.</returns>
        public static bool Includes(this LayerMask mask, GameObject obj)
        {
            return mask.Includes(obj.layer);
        }

        #endregion LayerMask Contains

        //
        // Private 
        //

        private class AscendingDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit h1, RaycastHit h2)
            {
                return h1.distance < h2.distance ? -1 : (h1.distance > h2.distance ? 1 : 0);
            }
        }

        private static AscendingDistanceComparer ascendDistance = new AscendingDistanceComparer();

        //TODO: Move to VectorExtensions and make public
        private static Vector3 AbsVec3(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private static float MaxVec3(Vector3 v)
        {
            return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
        }
    }
}