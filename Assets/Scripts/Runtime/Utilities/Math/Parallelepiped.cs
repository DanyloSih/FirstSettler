using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct Parallelepiped
    {
        private readonly Vector3Int _size;
        private readonly Vector3Int _extents;
        private readonly int _volume;
        private readonly int _width;
        private readonly int _widthAndHeight;

        public Vector3Int Size 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size; 
        }
        public Vector3Int Extents 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _extents; 
        }
        public int Volume 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _volume; 
        }
        public int WidthAndHeight 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _widthAndHeight; 
        }

        public Parallelepiped(Vector3Int size)
        {
            _size = size;
            _extents = size / 2;
            _width = size.x;
            _widthAndHeight = size.x * size.y;
            _volume = size.x * size.y * size.z;
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
            return point.x >= 0 && point.x < _size.x 
                && point.y >= 0 && point.y < _size.y
                && point.z >= 0 && point.z < _size.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSurfacePoint(Vector3Int point)
        {
            return (point.x % _size.x == 0 | point.y % _size.y == 0 | point.z % _size.z == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointToIndex(int x, int y, int z)
        {
            return x + y * _width + z * _widthAndHeight;
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
            int y = remainder / _width;
            int x = remainder % _width;

            return new Vector3Int(x, y, z);
        }
    }
}