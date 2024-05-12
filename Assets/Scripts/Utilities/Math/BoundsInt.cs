using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct BoundsInt
    {
        public readonly Vector3Int Center;
        public readonly Vector3Int Max;
        public readonly Vector3Int Min;
        public readonly RectPrismInt RectPrism;

        public BoundsInt(Vector3Int min, Vector3Int max)
        {
            RectPrism = new RectPrismInt(max - min);
            Center = min + RectPrism.HalfSize;
            Min = min;
            Max = max;
        }

        public BoundsInt(RectPrismInt rectPrism, Vector3Int center)
        {
            RectPrism = rectPrism;
            Center = center;
            Min = center - rectPrism.HalfSize;
            Max = center + rectPrism.HalfSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsContainsPoint(Vector3Int point)
        {
            return RectPrism.IsContainsPoint(point - Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSurfacePoint(Vector3Int point)
        {
            return RectPrism.IsSurfacePoint(point - Min);
        }
    }
}