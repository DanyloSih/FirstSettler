using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using Utilities.Math.Extensions;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public struct DeformMaskJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int Offset;
        [ReadOnly] public Vector3Int UnscaledGlobalDataPoint;
        [ReadOnly] public RectPrismInt EditingPrism;
        [ReadOnly] public RectPrismInt ChunkDataModel;
        [ReadOnly] public Vector3Int ChunkSize;
        [ReadOnly] public int HalfBrushSize;
        [ReadOnly] public float DeformFactor;
        [ReadOnly] public int MaterialHash;
        [ReadOnly] public NativeHashMap<int, IntPtr> ChunksDataPointersInsideEditArea;

        [WriteOnly] public NativeList<ChunkPoint>.ParallelWriter ChunkPoints;
        [WriteOnly] public int ItemsCount;

        public void Execute(int index)
        {
            Vector3Int pointerInArea = EditingPrism.IndexToPoint(index) + Offset;
            float unscaledDistance = pointerInArea.magnitude;
            float deformForce = unscaledDistance / HalfBrushSize;

            if (unscaledDistance < HalfBrushSize)
            {
                Vector3Int globalUnscaledDataPointPointer = UnscaledGlobalDataPoint + pointerInArea;
                Vector3Int pointedChunk = globalUnscaledDataPointPointer.GetElementwiseFloorDividedVector(ChunkSize);
                Vector3Int pointedChunkData = globalUnscaledDataPointPointer.GetElementwiseDividingRemainder(ChunkSize);
                pointedChunkData = FixNegativePoint(pointedChunkData, ChunkSize, out var chunkOffset);
                pointedChunk += chunkOffset;

                int chunkPositionHash = PositionHasher.GetPositionHash(pointedChunk);
                IntPtr rawDataStartPointer;
                if (!ChunksDataPointersInsideEditArea.TryGetValue(chunkPositionHash, out rawDataStartPointer))
                {
                    return;
                }

                int chunkVoxelOffset = ChunkDataModel.PointToIndex(pointedChunkData);

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
