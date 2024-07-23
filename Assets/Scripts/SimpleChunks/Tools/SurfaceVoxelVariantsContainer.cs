using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks.Tools
{
    [BurstCompile]
    [StructLayout(LayoutKind.Explicit)]
    public struct SurfaceVoxelVariantsContainer
    {
        public const int CAPACITY = 10;
        public const int SIGN_MASK = int.MaxValue;

        [FieldOffset(0)]
        private int _length;
        [FieldOffset(sizeof(int))]
        private Voxel _voxelNeighbors;
        [FieldOffset(sizeof(int) + Voxel.TYPE_SIZE * CAPACITY)]
        private byte _voxelNeighborsEnd;

        public Voxel MainVoxel => this[0];

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        /// <summary>
        /// First element "[0]" is always main voxel!
        /// </summary>
        public Voxel this[int id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    fixed (Voxel* ptr = &_voxelNeighbors)
                    {
                        return *(ptr + id);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                unsafe
                {
                    fixed (Voxel* ptr = &_voxelNeighbors)
                    {
                        *(ptr + id) = value;
                    }
                }
            }
        }

        /// <summary>
        /// Finds all coordinate variants of the current voxel in neighboring chunks if the voxel is neighboring.
        /// </summary>
        public SurfaceVoxelVariantsContainer(ChunkPoint mainVoxel, RectPrismInt chunkVoxelsPrism) : this()
        {
            _length = 0;

            Vector3Int voxelPosition = mainVoxel.LocalVoxelPosition;
            Vector3Int chunkPosition = mainVoxel.LocalChunkPosition;

            AddVoxel(chunkVoxelsPrism, chunkPosition, voxelPosition);

            Vector3Int chunkSizeInCubes = chunkVoxelsPrism.Size - Vector3Int.one;
            Int32List8 neighboringAxes = GetNeighboringAxes(voxelPosition, chunkSizeInCubes);

            int variantsIndex = neighboringAxes.Length;
            int axisAId = 0, axisBId = 0, axisCId = 0;
            switch (variantsIndex)
            {
                case 1:
                    axisAId = neighboringAxes.UnsafeGetValueAt(0);
                    AddVoxelWithOneConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId);
                    break;

                case 2:
                    axisAId = neighboringAxes.UnsafeGetValueAt(0);
                    axisBId = neighboringAxes.UnsafeGetValueAt(1);
                    AddVoxelWithOneConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId);
                    AddVoxelWithOneConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisBId);
                    AddVoxelWithTwoConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId, axisBId);
                    break;

                case 3:
                    axisAId = 0;
                    axisBId = 1;
                    axisCId = 2;
                   
                    AddVoxelWithOneConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisBId);
                    AddVoxelWithOneConvertedAxis(
                       chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId);
                    AddVoxelWithOneConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisCId);

                    AddVoxelWithTwoConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId, axisCId);
                    AddVoxelWithTwoConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisBId, axisCId);
                    AddVoxelWithTwoConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId, axisBId);

                    AddVoxelWithThreeConvertedAxis(
                        chunkVoxelsPrism, voxelPosition, chunkPosition, chunkSizeInCubes, axisAId, axisBId, axisCId);

                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVoxelWithThreeConvertedAxis(
           RectPrismInt chunkVoxelsPrism,
           Vector3Int voxelPosition,
           Vector3Int chunkPosition,
           Vector3Int chunkSizeInCubes,
           int axisAId,
           int axisBId,
           int axisCId)
        {
            Vector3Int neighboringVoxelPos = voxelPosition;
            Vector3Int neighboringChunkPos = chunkPosition;

            ConvertVoxelDataForAxis(
                voxelPosition, chunkSizeInCubes, axisAId, ref neighboringVoxelPos, ref neighboringChunkPos);

            ConvertVoxelDataForAxis(
                voxelPosition, chunkSizeInCubes, axisBId, ref neighboringVoxelPos, ref neighboringChunkPos);

            ConvertVoxelDataForAxis(
                voxelPosition, chunkSizeInCubes, axisCId, ref neighboringVoxelPos, ref neighboringChunkPos);
            AddVoxel(chunkVoxelsPrism, neighboringChunkPos, neighboringVoxelPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVoxelWithTwoConvertedAxis(
           RectPrismInt chunkVoxelsPrism,
           Vector3Int voxelPosition,
           Vector3Int chunkPosition,
           Vector3Int chunkSizeInCubes,
           int axisAId,
           int axisBId)
        {
            Vector3Int neighboringVoxelPos = voxelPosition;
            Vector3Int neighboringChunkPos = chunkPosition;

            ConvertVoxelDataForAxis(
                voxelPosition, chunkSizeInCubes, axisAId, ref neighboringVoxelPos, ref neighboringChunkPos);

            ConvertVoxelDataForAxis(
                voxelPosition, chunkSizeInCubes, axisBId, ref neighboringVoxelPos, ref neighboringChunkPos);
            AddVoxel(chunkVoxelsPrism, neighboringChunkPos, neighboringVoxelPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVoxelWithOneConvertedAxis(
            RectPrismInt chunkVoxelsPrism,
            Vector3Int voxelPosition,
            Vector3Int chunkPosition,
            Vector3Int chunkSizeInCubes,
            int axisAId)
        {
            Vector3Int neighboringVoxelPos = voxelPosition;
            Vector3Int neighboringChunkPos = chunkPosition;

            ConvertVoxelDataForAxis(voxelPosition, chunkSizeInCubes, axisAId, ref neighboringVoxelPos, ref neighboringChunkPos);
            AddVoxel(chunkVoxelsPrism, neighboringChunkPos, neighboringVoxelPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConvertVoxelDataForAxis(Vector3Int voxelPosition, Vector3Int chunkSizeInCubes, int axisAId, ref Vector3Int neighboringVoxelPos, ref Vector3Int neighboringChunkPos)
        {
            int a = voxelPosition[axisAId];
            if (a == 0)
            {
                neighboringVoxelPos[axisAId] = chunkSizeInCubes[axisAId];
                neighboringChunkPos[axisAId] -= 1;
            }
            else if (a >= chunkSizeInCubes[axisAId])
            {
                neighboringVoxelPos[axisAId] = 0;
                neighboringChunkPos[axisAId] += 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVoxel(RectPrismInt chunkVoxelsPrism, Vector3Int chunkPosition, Vector3Int voxelPosition)
        {
            this[_length] = ChunkPoint.ToVoxel(chunkPosition, voxelPosition, chunkVoxelsPrism);
            _length++;
        }

        private Int32List8 GetNeighboringAxes(Vector3Int voxelPosition, Vector3Int chunkSizeInCubes)
        {
            Int32List8 buffer = new Int32List8();

            if (voxelPosition.x == chunkSizeInCubes.x || voxelPosition.x == 0)
            {
                buffer.AddValue(0);
            }

            if (voxelPosition.y == chunkSizeInCubes.y || voxelPosition.y == 0)
            {
                buffer.AddValue(1);
            }

            if (voxelPosition.z == chunkSizeInCubes.z || voxelPosition.z == 0)
            {
                buffer.AddValue(2);
            }

            return buffer;
        }
    }
}
