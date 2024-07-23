using System;
using UnityEngine;

namespace Utilities.Math.Extensions
{

    public static class Vector3Extensions
    {
        public static Vector3 GetElementwiseDividedVector(this Vector3 numerator, Vector3 denominator)
        {
            return new Vector3(
               numerator.x / denominator.x,
               numerator.y / denominator.y,
               numerator.z / denominator.z);
        }

        public static Vector3 GetElementwiseDividingRemainder(this Vector3 numerator, Vector3 denominator)
        {
            return new Vector3(numerator.x % denominator.x, numerator.y % denominator.y, numerator.z % denominator.z);
        }
    }
}