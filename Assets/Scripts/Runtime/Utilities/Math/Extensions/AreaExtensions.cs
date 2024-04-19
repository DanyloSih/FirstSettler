using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math.Extensions
{
    public static class AreaExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsContainsPoint(this IAreaInt area, Vector3Int point)
        {
            return area.AbstractShape.IsContainsPoint(point - area.Anchor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PointToIndex(this IAreaInt area, Vector3Int point)
        {
            return area.AbstractShape.PointToIndex(point - area.Anchor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int IndexToPoint(this IAreaInt area, int index)
        {
            return area.AbstractShape.IndexToPoint(index) + area.Anchor;
        }

        public static IEnumerable<Vector3Int> GetEveryPoint(this IAreaInt area)
        {
            for (int i = 0; i < area.AbstractShape.Volume; i++)
            {
                yield return area.AbstractShape.IndexToPoint(i) + area.Anchor;
            }
        }

        public static IEnumerable<int> GetEveryIndex(this IAreaInt area)
        {
            for (int i = 0; i < area.AbstractShape.Volume; i++)
            {
                yield return i;
            }
        }
    }
}