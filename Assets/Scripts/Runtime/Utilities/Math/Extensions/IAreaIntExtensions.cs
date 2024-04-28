using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math.Extensions
{

    public static class IAreaIntExtensions
    {
        public static void DrawGizmos(this IAreaInt area, Color gizmosColor, Vector3 size, Vector3 offset, bool isWire)
        {
            Vector3 halfSize = size;
            var previousColor = Gizmos.color;
            Gizmos.color = gizmosColor;
            foreach (var point in area.GetEveryPoint())
            {
                Vector3 scaledPoint = point;
                scaledPoint.Scale(size);
                if (isWire)
                {
                    Gizmos.DrawWireCube(scaledPoint + halfSize + offset, size);
                }
                else
                {
                    Gizmos.DrawCube(scaledPoint + halfSize + offset, size);
                }
            }
            Gizmos.color = previousColor;
        }

        public static ShapeIntArea<DisposableArbitraryShapeInt> BooleanCutViaOtherArea(
            this IAreaInt area, IAreaInt otherArea)
        {
            int totalVolume = area.AbstractShape.Volume;
            NativeHashMap<Vector3Int, int> pointToIndexAssociations
                = new(totalVolume, Allocator.Persistent);

            NativeHashMap<int, Vector3Int> indexToPointAssociations
                = new(totalVolume, Allocator.Persistent);

            int counter = 0;
            for (int i = 0; i < totalVolume; i++)
            {
                var targetAreaPoint = area.IndexToPoint(i);
                var shapePoint = area.AbstractShape.IndexToPoint(i);
                if (!otherArea.IsContainsPoint(targetAreaPoint))
                {
                    pointToIndexAssociations.Add(shapePoint, counter);
                    indexToPointAssociations.Add(counter, shapePoint);
                    counter++;
                }
            }

            return new ShapeIntArea<DisposableArbitraryShapeInt>(
                new DisposableArbitraryShapeInt(pointToIndexAssociations, indexToPointAssociations), area.Anchor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsContainsIndex(this IAreaInt area, int index)
        {
            return area.AbstractShape.IsContainsIndex(index);
        }

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