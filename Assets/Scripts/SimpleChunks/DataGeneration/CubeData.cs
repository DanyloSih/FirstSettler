using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;

namespace SimpleChunks.DataGeneration
{
    [StructLayout(LayoutKind.Explicit)]
    public struct CubeData
    {
        public const int CAPACITY = 8;

        [FieldOffset(0)]
        public VoxelData ArrayStart;
        [FieldOffset(CAPACITY * VoxelData.STRUCT_SIZE)]
        private float _surface;

        public CubeData(float surface) : this()
        {
            _surface = surface;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CubeData Initialize(UnsafeNativeArray<VoxelData> rawData, RectPrismInt rawDataRect, Vector3Int pos)
        {
            this[0] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x, pos.y, pos.z))];
            this[1] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x + 1, pos.y, pos.z))];
            this[2] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x + 1, pos.y + 1, pos.z))];
            this[3] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x, pos.y + 1, pos.z))];
            this[4] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x, pos.y, pos.z + 1))];
            this[5] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x + 1, pos.y, pos.z + 1))];
            this[6] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x + 1, pos.y + 1, pos.z + 1))];
            this[7] = rawData[rawDataRect.PointToIndex(new Vector3Int(pos.x, pos.y + 1, pos.z + 1))];

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvisible()
        {
            int configIndex = GetConfigurationIndex();
            return configIndex == 0 || configIndex == 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetConfigurationIndex()
        {
            int configIndex = 0;

            if (this[0].Volume <= _surface)
                configIndex |= 1 << 0;
            if (this[1].Volume <= _surface)
                configIndex |= 1 << 1;
            if (this[2].Volume <= _surface)
                configIndex |= 1 << 2;
            if (this[3].Volume <= _surface)
                configIndex |= 1 << 3;
            if (this[4].Volume <= _surface)
                configIndex |= 1 << 4;
            if (this[5].Volume <= _surface)
                configIndex |= 1 << 5;
            if (this[6].Volume <= _surface)
                configIndex |= 1 << 6;
            if (this[7].Volume <= _surface)
                configIndex |= 1 << 7;

            return configIndex;
        }

        public VoxelData this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckIsOutOfRange(index);
                unsafe
                {
                    fixed (VoxelData* ptr = &ArrayStart)
                    {
                        return *(ptr + index);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckIsOutOfRange(index);
                unsafe
                {
                    fixed (VoxelData* ptr = &ArrayStart)
                    {
                        *(ptr + index) = value;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckIsOutOfRange(int index)
        {
            if (index < 0 || index >= CAPACITY)
            {
                throw new System.IndexOutOfRangeException();
            }
        }
    }
}
