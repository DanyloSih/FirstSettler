using SimpleChunks.DataGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;

namespace SimpleChunks.Tools
{
    [BurstCompile]
    public struct DeformMaskJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int Offset;
        [ReadOnly] public Vector3Int UnscaledGlobalDataPoint;
        [ReadOnly] public RectPrismInt EditingPrism;
        [ReadOnly] public RectPrismInt ChunkVoxelsRect;
        [ReadOnly] public Vector3Int ChunkSizeInCubes;
        [ReadOnly] public int HalfBrushSize;
        [ReadOnly] public float DeformFactor;
        [ReadOnly] public int MaterialHash;
        [ReadOnly] public float Surface;
        [ReadOnly] public NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>.ReadOnly ChunksDataPointersInsideEditArea;

        [WriteOnly] public NativeList<ChunkPointWithData>.ParallelWriter ChunkPoints;
        [WriteOnly] public int ItemsCount;

        public void Execute(int index)
        {
            Vector3Int pointerInArea = EditingPrism.IndexToPoint(index) + Offset;
            float unscaledDistance = Vector3Int.Distance(new Vector3Int(HalfBrushSize, HalfBrushSize, HalfBrushSize) + Offset, pointerInArea);
            float deformForce = 1f - (unscaledDistance / HalfBrushSize);
            
            if (unscaledDistance < HalfBrushSize)
            {
                ChunkPoint chunkPoint = ChunksMath.GetChunkPoint(UnscaledGlobalDataPoint + pointerInArea, ChunkSizeInCubes);
                long chunkPositionHash = PositionLongHasher.GetHashFromPosition(chunkPoint.LocalChunkPosition);
                if (!ChunksDataPointersInsideEditArea.TryGetValue(chunkPositionHash, out var rawData))
                {
                    return;
                }

                int voxelId = ChunkVoxelsRect.PointToIndex(chunkPoint.LocalVoxelPosition);
                VoxelData data = rawData[voxelId];

                if (data.MaterialHash == 0)
                {
                    return;
                }

                data.Volume = Mathf.Clamp01(data.Volume + DeformFactor * deformForce);
                rawData[voxelId] = data;

                //ChunkPoints.AddNoResize(new ChunkPointWithData(chunkPoint, data));
            }
        }

        
    }
}
