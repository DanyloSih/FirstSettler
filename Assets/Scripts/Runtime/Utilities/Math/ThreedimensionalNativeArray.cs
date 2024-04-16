using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Utilities.Math
{
    public struct ThreedimensionalNativeArray<T> : IDisposable
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
        public Parallelepiped Parallelepiped => _parallelepiped;

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

        /// <param name="minPosition">Start position of data copying.</param>
        /// <param name="resultArray">The array into which the data will be copied</param>
        public JobHandle CreateCopyPartJob(
            Vector3Int minPosition,
            ThreedimensionalNativeArray<T> resultArray,
            int innerLoopBatchCount = 4)
        {
            var copyArrayPartJob = new CopyArrayPartJob<T>(this, resultArray, minPosition);
            JobHandle copyJobHandle = copyArrayPartJob.Schedule(resultArray._parallelepiped.Volume, innerLoopBatchCount);
            return copyJobHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNewRawData(ref NativeArray<T> newRawData)
        {
            RawData = newRawData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int x, int y, int z)
        {
            int id = PositionToIndex(x, y, z);
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
            int id = PositionToIndex(x, y, z);
            RawData[id] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int index, T value)
        {
            RawData[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PositionToIndex(Vector3Int position)
        {
            return _parallelepiped.PointToIndex(position.x, position.y, position.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PositionToIndex(int x, int y, int z)
        {
            return _parallelepiped.PointToIndex(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToPostion(int index)
        {
            return _parallelepiped.IndexToPoint(index);
        }

        public void Dispose()
        {
            RawData.Dispose();
        }
    }
}
