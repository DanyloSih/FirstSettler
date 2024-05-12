using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math.Extensions
{
    public static class IShapeIntExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsContainsIndex(this IShapeInt shape, int index)
        {
            return index >= 0 && index < shape.Volume;
        }

        public static IEnumerable<Vector3Int> GetEveryPoint(this IShapeInt shape)
        {
            for (int i = 0; i < shape.Volume; i++)
            {
                yield return shape.IndexToPoint(i);
            }
        }

        public static IEnumerable<int> GetEveryIndex(this IShapeInt shape)
        {
            for(int i = 0; i < shape.Volume; i++)
            {
                yield return i;
            }
        }
    }
}