using SimpleChunks.DataGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;

namespace SimpleChunks.Tools
{
    public struct SphericalDeformMaskJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int Offset;
        [ReadOnly] public Vector3Int UnscaledGlobalDataPoint;
        [ReadOnly] public RectPrismInt EditingPrism;
        [ReadOnly] public RectPrismInt ChunkVoxelsPrism;
        [ReadOnly] public Vector3Int ChunkSizeInCubes;
        [ReadOnly] public int HalfBrushSize;
        [ReadOnly] public float DeformFactor;
        [ReadOnly] public int MaterialHash;
        [ReadOnly] public float Surface;
        [ReadOnly] public NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>.ReadOnly ChunksDataPointersInsideEditArea;

        public void Execute(int index)
        {
            Vector3Int pointerInArea = EditingPrism.IndexToPoint(index) + Offset;
            float unscaledDistance = Vector3Int.Distance(new Vector3Int(HalfBrushSize, HalfBrushSize, HalfBrushSize) + Offset, pointerInArea);
            float deformForce = 1f - (unscaledDistance / HalfBrushSize);

            if (unscaledDistance < HalfBrushSize)
            {
                ChunkPoint chunkPoint = ChunksMath.GetChunkPoint(UnscaledGlobalDataPoint + pointerInArea, ChunkSizeInCubes);
                SurfaceVoxelVariantsContainer voxelsContainer = new (chunkPoint, ChunkVoxelsPrism);
                Voxel centerVoxel = voxelsContainer.MainVoxel;

                if (voxelsContainer.Length == 8)
                {

                }

                if (!ChunksDataPointersInsideEditArea.TryGetValue(
                    centerVoxel.ChunkHash, out var rawData))
                {
                    return;
                }

                VoxelData data = rawData[centerVoxel.VoxelID];

                if (data.MaterialHash == 0)
                {
                    return;
                }

                data.Volume = Mathf.Clamp01(data.Volume + DeformFactor * deformForce);

                for (int i = 0; i < voxelsContainer.Length; i++)
                {
                    if (ChunksDataPointersInsideEditArea.TryGetValue(voxelsContainer[i].ChunkHash, out rawData))
                    {
                        rawData[voxelsContainer[i].VoxelID] = data;
                    }
                }
            }
        }

        
    }
}
