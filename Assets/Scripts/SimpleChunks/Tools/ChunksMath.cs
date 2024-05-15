using System.Runtime.CompilerServices;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Math.Extensions;

namespace SimpleChunks.Tools
{
    public static class ChunksMath
    {
        public static ChunkPoint GetChunkPoint(Vector3Int unscaledGlobalVoxelPosition, Vector3Int chunkSizeInCubes)
        {
            Vector3Int globalUnscaledDataPointPointer = unscaledGlobalVoxelPosition;
            Vector3Int pointedChunk = globalUnscaledDataPointPointer.GetElementwiseFloorDividedVector(chunkSizeInCubes);
            Vector3Int pointedChunkData = globalUnscaledDataPointPointer.GetElementwiseDividingRemainder(chunkSizeInCubes);
            pointedChunkData = FixNegativePoint(pointedChunkData, chunkSizeInCubes, out var chunkOffset);
            pointedChunk += chunkOffset;

            return new ChunkPoint(pointedChunk, pointedChunkData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> GetChunksDataPointersInsideArea(
            ShapeIntArea<RectPrismInt> area, 
            Vector3Int chunksSize,
            ChunksContainer chunksContainer, 
            out NativeList<Vector3Int> affectedPositions)
        {
            affectedPositions = new NativeList<Vector3Int>(Allocator.Persistent);
            Vector3Int affectedAreaSize = area.Shape.Size;
            int maxXChunks = affectedAreaSize.x / chunksSize.x + 1;
            int maxYChunks = affectedAreaSize.y / chunksSize.y + 1;
            int maxZChunks = affectedAreaSize.z / chunksSize.z + 1;
            int maxAffectedChunksCount = maxXChunks * maxYChunks * maxZChunks;

            Vector3Int min = area.Anchor;
            Vector3Int max = area.Anchor + area.Shape.Size;

            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> pointers
                = new (maxAffectedChunksCount, Allocator.Persistent);

            for (int y = Mathf.FloorToInt((float)min.y / chunksSize.y) * chunksSize.y; y < max.y; y += chunksSize.y)
            {
                for (int x = Mathf.FloorToInt((float)min.x / chunksSize.x) * chunksSize.x; x < max.x; x += chunksSize.x)
                {
                    for (int z = Mathf.FloorToInt((float)min.z / chunksSize.z) * chunksSize.z; z < max.z; z += chunksSize.z)
                    {
                        int localChunkX = x / chunksSize.x;
                        int localChunkY = y / chunksSize.y;
                        int localChunkZ = z / chunksSize.z;
                        long positionHash = PositionLongHasher.GetHashFromPosition(localChunkX, localChunkY, localChunkZ);
                        chunksContainer.TryGetValue(positionHash, out var chunk);

                        if (chunk == null)
                        {
                            continue;
                        }

                        unsafe
                        {
                            affectedPositions.Add(new Vector3Int(localChunkX, localChunkY, localChunkZ));
                            pointers.Add(positionHash, new UnsafeNativeArray<VoxelData>(chunk.ChunkData.RawData));
                        };
                    }
                }
            }

            return pointers;
        }

        /// <summary>
        /// This method allows you to get the correct position of a voxel within 
        /// a chunk if the global position of that voxel contains a negative coordinate.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3Int FixNegativePoint(Vector3Int globalVoxelPoint, Vector3Int chunkSize, out Vector3Int chunkOffset)
        {
            chunkOffset = new Vector3Int();
            if (globalVoxelPoint.x < 0)
            {
                globalVoxelPoint.x = chunkSize.x + globalVoxelPoint.x;
                chunkOffset.x = -1;
            }

            if (globalVoxelPoint.y < 0)
            {
                globalVoxelPoint.y = chunkSize.y + globalVoxelPoint.y;
                chunkOffset.y = -1;
            }

            if (globalVoxelPoint.z < 0)
            {
                globalVoxelPoint.z = chunkSize.z + globalVoxelPoint.z;
                chunkOffset.z = -1;
            }

            return globalVoxelPoint;
        }
    }
}
