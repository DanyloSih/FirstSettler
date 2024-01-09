using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct Parallelepiped
    {
        public Vector3Int Size { get; private set; }
        public Vector3Int Extents { get; private set; }
        public int Volume { get; private set; }
        public int SurfaceArea { get; private set; }

        private int _width;
        private int _widthAndHeight;

        public Parallelepiped(Vector3Int size)
        {
            Size = size;
            Extents = Size / 2;
            _width = size.x;
            _widthAndHeight = size.x * size.y;
            Volume = Size.x * Size.y * Size.z;
            SurfaceArea = 2 * (Size.x * Size.y + Size.x * Size.z + Size.y * Size.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VoxelPositionToIndex(int x, int y, int z)
        {
            return x + y * _width + z * _widthAndHeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToVoxelPosition(int index)
        {
            int z = index / _widthAndHeight;
            int remainder = index % _widthAndHeight;
            int y = remainder / _width;
            int x = remainder % _width;

            return new Vector3Int(x, y, z);
        }
    }
}