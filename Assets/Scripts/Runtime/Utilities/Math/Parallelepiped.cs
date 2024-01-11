using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct Parallelepiped
    {
        private Vector3Int _size;
        private Vector3Int _extents;
        private int _volume;

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

        private int _width;
        private int _widthAndHeight;

        public Parallelepiped(Vector3Int size)
        {
            _size = size;
            _extents = size / 2;
            _width = size.x;
            _widthAndHeight = size.x * size.y;
            _volume = size.x * size.y * size.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSurfacePoint(Vector3Int point)
        {
            return (point.x % _size.x == 0 | point.y % _size.y == 0 | point.z % _size.z == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VoxelPositionToIndex(int x, int y, int z)
        {
            return x + y * _width + z * _widthAndHeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VoxelPositionToIndex(Vector3Int point)
        {
            return VoxelPositionToIndex(point.x, point.y, point.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToVoxelPosition(int index)
        {
            int z = index / WidthAndHeight;
            int remainder = index % WidthAndHeight;
            int y = remainder / _width;
            int x = remainder % _width;

            return new Vector3Int(x, y, z);
        }
    }
}