using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Utilities.Math
{
    public struct ThreedimensionalNativeArray<T> : IDisposable
        where T : struct
    {
        public NativeArray<T> RawData;
        private int _dataAreaStartIndex;
        private int _dataAreaEndIndex;
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;
        private readonly Vector3Int _size;
        private readonly RectPrismInt _rectPrism;
        private readonly int _widthAndHeight;
        private readonly int _fullLength;

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public Vector3Int Size => _size;
        public int WidthAndHeight => _widthAndHeight;
        public int FullLength => _fullLength;
        public RectPrismInt RectPrism => _rectPrism;

        public ThreedimensionalNativeArray(int width, int height, int depth)
            : this(new Vector3Int(width, height, depth))
        {

        }

        public ThreedimensionalNativeArray(Vector3Int size) 
            : this(new NativeArray<T>(size.x * size.y * size.z, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                  size, 0)
        {
           
        }

        /// <param name="rawData"></param>
        /// <param name="size"></param>
        /// <param name="startIndex">Determines from which element in the <paramref name="rawData"/> starts data area
        /// for this object.</param>
        public ThreedimensionalNativeArray(NativeArray<T> rawData, Vector3Int size, int startIndex = 0)
        {
            _width = size.x;
            _height = size.y;
            _depth = size.z;
            _size = new Vector3Int(_width, _height, _depth);
            _rectPrism = new RectPrismInt(_size);
            _widthAndHeight = _width * _height;
            _fullLength = _width * _height * _depth;

            _dataAreaStartIndex = startIndex;
            _dataAreaEndIndex = _dataAreaStartIndex + _fullLength;

            CheckDataAreaBordersCorectness(rawData, size, startIndex, _dataAreaEndIndex);
            RawData = rawData;
        }

        private static void CheckDataAreaBordersCorectness(NativeArray<T> rawData, Vector3Int size, int startIndex, int endIndex)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (endIndex > rawData.Length)
            {
                throw new ArgumentException($"Data area end index greater then {nameof(rawData)} length!" +
                    $"Check correctness of this parameters: \"{nameof(size)}\", \"{nameof(startIndex)}\"");
            }
        }

        /// <param name="minPosition">Start position of data copying.</param>
        /// <param name="resultArray">The array into which the data will be copied</param>
        public JobHandle CreateCopyPartJob(
            Vector3Int minPosition,
            ThreedimensionalNativeArray<T> resultArray,
            int innerLoopBatchCount = 4)
        {
            var copyArrayPartJob = new CopyArrayPartJob<T>(this, resultArray, minPosition);
            JobHandle copyJobHandle = copyArrayPartJob.Schedule(resultArray._rectPrism.Volume, innerLoopBatchCount);
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
        public T GetValue(Vector3Int position)
        {
            int id = PositionToIndex(position);
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
            return _rectPrism.PointToIndex(position.x, position.y, position.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PositionToIndex(int x, int y, int z)
        {
            return _rectPrism.PointToIndex(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToPostion(int index)
        {
            return _rectPrism.IndexToPoint(index);
        }

        public void Dispose()
        {
            if (RawData.IsCreated)
            {
                RawData.Dispose();
            }
        }
    }
}
