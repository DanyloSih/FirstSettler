using UnityEngine;

namespace FirstSettler.Extensions
{
    public static class Vector3IntExtensions
    {
        public static Vector3Int GetElementwiseDividedVector(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
        }

        public static Vector3Int GetElementwiseDividingRemainder(this Vector3Int numerator, Vector3Int denominator)
        {
            return new Vector3Int(numerator.x % denominator.x, numerator.y % denominator.y, numerator.z % denominator.z);
        }
    }
}