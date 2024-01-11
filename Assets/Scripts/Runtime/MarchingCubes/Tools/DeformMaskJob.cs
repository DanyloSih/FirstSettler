using System;
using System.Runtime.CompilerServices;
using FirstSettler.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public struct DeformMaskJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int Offset;
        [ReadOnly] public Vector3Int UnscaledGlobalDataPoint;
        [ReadOnly] public Parallelepiped EditingParallelepiped;
        [ReadOnly] public Parallelepiped ChunkDataModel;
        [ReadOnly] public Vector3Int ChunkSize;
        [ReadOnly] public int HalfBrushSize;
        [ReadOnly] public float DeformFactor;
        [ReadOnly] public int MaterialHash;
        [ReadOnly] public NativeHashMap<long, IntPtr> ChunksDataPointersInsideEditArea;

        [WriteOnly] public NativeList<ChunkPoint>.ParallelWriter ChunkPoints;
        [WriteOnly] public int ItemsCount;

        public void Execute(int index)
        {
            Vector3Int pointerInArea = EditingParallelepiped.IndexToVoxelPosition(index) + Offset;
            float unscaledDistance = pointerInArea.magnitude;
            float deformForce = unscaledDistance / HalfBrushSize;

            if (unscaledDistance < HalfBrushSize)
            {
                Vector3Int globalUnscaledDataPointPointer = UnscaledGlobalDataPoint + pointerInArea;
                Vector3Int pointedChunk = globalUnscaledDataPointPointer.GetElementwiseDividedVector(ChunkSize);
                Vector3Int pointedChunkData = globalUnscaledDataPointPointer.GetElementwiseDividingRemainder(ChunkSize);
                pointedChunkData = FixNegativePoint(pointedChunkData, ChunkSize, out var chunkOffset);
                pointedChunk += chunkOffset;

                long chunkPositionHash = PositionHasher.GetPositionHash(
                    pointedChunk.x, pointedChunk.y, pointedChunk.z);

                IntPtr rawDataStartPointer;
                if (!ChunksDataPointersInsideEditArea.TryGetValue(chunkPositionHash, out rawDataStartPointer))
                {
                    return;
                }

                int chunkVoxelOffset = ChunkDataModel.VoxelPositionToIndex(pointedChunkData);

                VoxelData data;
                unsafe
                {
                    VoxelData* rawData = (VoxelData*)rawDataStartPointer.ToPointer();
                    data = rawData[chunkVoxelOffset];
                }

                if (data.MaterialHash == 0)
                {
                    return;
                }

                data.Volume = Mathf.Clamp01(DeformFactor);

                ChunkPoints.AddNoResize(new ChunkPoint(pointedChunk, pointedChunkData, data));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3Int FixNegativePoint(Vector3Int point, Vector3Int chunkSize, out Vector3Int chunkOffset)
        {
            chunkOffset = new Vector3Int();
            if (point.x < 0)
            {
                point.x = chunkSize.x + point.x;
                chunkOffset.x = -1;
            }

            if (point.y < 0)
            {
                point.y = chunkSize.y + point.y;
                chunkOffset.y = -1;
            }

            if (point.z < 0)
            {
                point.z = chunkSize.z + point.z;
                chunkOffset.z = -1;
            }

            return point;
        }
    }
}
