using UnityEngine;

namespace Utilities.Math.Extensions
{
    public static class Vector3IntExtensions
    {
        public static Vector3Int GetElementwiseRoundDividedVector(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(
                Mathf.RoundToInt(numerator.x / (float)denominator.x),
                Mathf.RoundToInt(numerator.y / (float)denominator.y),
                Mathf.RoundToInt(numerator.z / (float)denominator.z));
        }

        public static Vector3Int GetElementwiseCeilDividedVector(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(
                Mathf.CeilToInt(numerator.x / (float)denominator.x), 
                Mathf.CeilToInt(numerator.y / (float)denominator.y), 
                Mathf.CeilToInt(numerator.z / (float)denominator.z));
        }

        public static Vector3Int GetElementwiseFloorDividedVector(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
        }

        public static Vector3Int GetElementwiseDividingRemainder(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(numerator.x % denominator.x, numerator.y % denominator.y, numerator.z % denominator.z);
        }
    }
}