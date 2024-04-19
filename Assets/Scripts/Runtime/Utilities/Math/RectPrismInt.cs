using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct RectPrismInt : IShapeInt
    {
        public readonly Vector3Int Size;
        public readonly Vector3Int HalfSize;
        public readonly int Width;
        public readonly int WidthAndHeight;
        private readonly int _volume;

        public int Volume => _volume;

        public RectPrismInt(Vector3Int size)
        {
            Size = size;
            HalfSize = size / 2;
            Width = size.x;
            WidthAndHeight = size.x * size.y;
            _volume = size.x * size.y * size.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetEveryIndex()
        {
            for (int i = 0; i < Volume; i++)
            {
                yield return i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Vector3Int> GetEveryPoint()
        {
            for (int i = 0; i < Volume; i++)
            {
                yield return IndexToPoint(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsContainsPoint(Vector3Int point)
        {
            return point.x >= 0 && point.x < Size.x 
                && point.y >= 0 && point.y < Size.y
                && point.z >= 0 && point.z < Size.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSurfacePoint(Vector3Int point)
        {
            return (point.x % Size.x == 0 | point.y % Size.y == 0 | point.z % Size.z == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointToIndex(int x, int y, int z)
        {
            return x + y * Width + z * WidthAndHeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointToIndex(Vector3Int point)
        {
            return PointToIndex(point.x, point.y, point.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToPoint(int index)
        {
            int z = index / WidthAndHeight;
            int remainder = index % WidthAndHeight;
            int y = remainder / Width;
            int x = remainder % Width;

            return new Vector3Int(x, y, z);
        }
    }
}