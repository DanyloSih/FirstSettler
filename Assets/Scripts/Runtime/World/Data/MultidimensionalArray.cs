using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FirstSettler.Extensions;
using UnityEngine;
using Utilities.Math;

namespace World.Data
{
    public class MultidimensionalArray<T>
    {
        private readonly T[] _data;
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;
        private readonly Vector3Int _size;
        private readonly Parallelepiped _parallelepiped;
        private readonly int _widthAndHeight;
        private readonly int _fullLength;
        private ComputeBuffer _voxelsBuffer;

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public Vector3Int Size => _size;
        public int WidthAndHeight => _widthAndHeight;
        public int FullLength => _fullLength;
        public  T[] Data => _data;

        public MultidimensionalArray(Vector3Int size) 
            : this(size.x, size.y, size.z)
        {
           
        }

        public MultidimensionalArray(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _size = new Vector3Int(_width, _height, _depth);
            _parallelepiped = new Parallelepiped(_size);
            _widthAndHeight = _width * _height;
            _fullLength = _width * _height * _depth;
            _data = new T[_fullLength];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetDataFromVoxelsBuffer(ComputeBuffer dataComputeBuffer)
        {
            dataComputeBuffer.GetData(_data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComputeBuffer GetOrCreateVoxelsDataBuffer()
        {
            _voxelsBuffer = _voxelsBuffer ?? ComputeBufferExtensions.Create(FullLength, typeof(T));
            _voxelsBuffer.SetData(_data);
            return _voxelsBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int x, int y, int z)
        {
            int id = XYZToIndex(x, y, z);
            return _data[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int index)
        {
            return _data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int y, int z, T value)
        {
            int id = XYZToIndex(x, y, z);
            _data[id] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int index, T value)
        {
            _data[index] = value;
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
