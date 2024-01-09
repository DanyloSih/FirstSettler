using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;

namespace World.Data
{
    public struct ThreedimensionalNativeArray<T> 
        where T : struct
    {
        public NativeArray<T> RawData;

        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;
        private readonly Vector3Int _size;
        private readonly Parallelepiped _parallelepiped;
        private readonly int _widthAndHeight;
        private readonly int _fullLength;

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public Vector3Int Size => _size;
        public int WidthAndHeight => _widthAndHeight;
        public int FullLength => _fullLength;


        public ThreedimensionalNativeArray(Vector3Int size) 
            : this(size.x, size.y, size.z)
        {
           
        }

        public ThreedimensionalNativeArray(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _size = new Vector3Int(_width, _height, _depth);
            _parallelepiped = new Parallelepiped(_size);
            _widthAndHeight = _width * _height;
            _fullLength = _width * _height * _depth;
            RawData = new NativeArray<T>(_fullLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNewRawData(ref NativeArray<T> newRawData)
        {
            RawData = newRawData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int x, int y, int z)
        {
            int id = XYZToIndex(x, y, z);
            return RawData[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int index)
        {
            return RawData[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int y, int z, T value)
        {
            int id = XYZToIndex(x, y, z);
            RawData[id] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int index, T value)
        {
            RawData[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int XYZToIndex(int x, int y, int z)
        {
            return _parallelepiped.VoxelPositionToIndex(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToXYZ(int index)
        {
            return _parallelepiped.IndexToVoxelPosition(index);
        }
    }
}
