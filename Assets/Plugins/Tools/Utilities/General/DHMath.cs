using System.Linq;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// </summary>
    public static class DHMath
    {
        public enum NegativeValueHandler { MakePositive, KeepNegative, Truncate, ZeroOut }

        public static Quaternion Inverted(this Quaternion quat) => Quaternion.Inverse(quat);

        /// <summary>
        /// Performs a lerp and evaluates it on the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="percent"></param>
        /// <param name="normalizeTime"></param>
        public static Vector3 LerpWithCurve(this AnimationCurve curve, Vector3 start, Vector3 end, float percent, bool normalizeTime = false)
        {
            return Vector3.Lerp(start, end, CalcCurveWeight(curve, percent, normalizeTime));
        }

        public static Vector3 LerpWithCurveUnclamped(this AnimationCurve curve, Vector3 start, Vector3 end, float percent, bool normalizeTime = false)
        {
            return Vector3.LerpUnclamped(start, end, CalcCurveWeight(curve, percent, normalizeTime));
        }

        public static float CalcCurveWeight(this AnimationCurve curve, float percent, bool normalizeTime)
        {
            percent = CalcCurvePercent(curve, percent, normalizeTime);
            return curve.Evaluate(percent);
        }

        public static float CalcCurvePercent(this AnimationCurve curve, float percent, bool normalizeTime)
        {
            if(normalizeTime)
            {
                percent /= curve.keys.Last().time;
            }

            return percent;
        }

        
        /// <summary>
        /// Makes a copy of the curve and squares all the time values on its keyframes.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="avoidFlip"></param>
        /// <returns></returns>
        public static AnimationCurve GenerateSquaredTimeCopy(this AnimationCurve curve, NegativeValueHandler flipHandler= NegativeValueHandler.KeepNegative)
        {
            AnimationCurve newCurve = new AnimationCurve(curve.keys);
            newCurve.SquareTimeValuesNonAlloc();
            return newCurve;
        }

        /// <summary>
        /// Squares the time value of each keyframe. To avoid it flipping negative falls, set the avoidFlip flag to true
        /// </summary>
        /// <param name="curve"></param>
        public static void SquareTimeValuesNonAlloc(this AnimationCurve curve, NegativeValueHandler flipHandler = NegativeValueHandler.KeepNegative)
        {
            //TODO: when time permits

            //int startIndex = 0;
            //if(flipHandler == NegativeValueHandler.Truncate)
            //{
            //    float currentTime = curve.keys[0].time;
            //    for(int i = 0; i < curve.keys.Length && currentTime < 0; i++, currentTime = curve.keys[i].time)
            //    {
            //        startIndex = i + 1;
                    
            //    }
            //}

            for(int i = 0; i < curve.keys.Length; i++)
            {
                float originalTime = curve.keys[i].time;
                float sign = 1f;

                if(originalTime < 0f)
                {
                    bool skip = false;
                    switch(flipHandler)
                    {
                        case NegativeValueHandler.KeepNegative:
                            sign = -1f;
                            break;
                        case NegativeValueHandler.MakePositive:
                            sign = 1f;
                            break;
                        case NegativeValueHandler.Truncate:
                            ConsoleLog.LogError("Truncation Mode is not yet implemented!");
                            sign = 0f;
                            break;
                        case NegativeValueHandler.ZeroOut:
                            sign = 0f;
                            break;
                    }

                    if(skip) continue;
                }


                curve.keys[i].time = originalTime * originalTime * sign;
            }
        }
    }
}
