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
            Vector3Int chunksSizeInCubes,
            ChunksContainer chunksContainer, 
            out NativeList<Vector3Int> affectedPositions)
        {
            affectedPositions = new NativeList<Vector3Int>(Allocator.Persistent);

            Vector3Int min = area.Anchor;
            Vector3Int max = area.Anchor + area.Shape.Size;

            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> pointers
                = new (8, Allocator.Persistent);

            Vector3Int startChunkPosition = new Vector3Int(
               Mathf.FloorToInt((float)min.x / chunksSizeInCubes.x) * chunksSizeInCubes.x,
               Mathf.FloorToInt((float)min.y / chunksSizeInCubes.y) * chunksSizeInCubes.y,
               Mathf.FloorToInt((float)min.z / chunksSizeInCubes.z) * chunksSizeInCubes.z);

            Vector3Int negativeNeighboringFactor = new Vector3Int(
                (min.x % chunksSizeInCubes.x == 0 ? 1 : 0) * -chunksSizeInCubes.x,
                (min.y % chunksSizeInCubes.y == 0 ? 1 : 0) * -chunksSizeInCubes.y,
                (min.z % chunksSizeInCubes.z == 0 ? 1 : 0) * -chunksSizeInCubes.z);

            startChunkPosition += negativeNeighboringFactor;

            for (int y = startChunkPosition.y; y < max.y; y += chunksSizeInCubes.y)
            {
                for (int x = startChunkPosition.x; x < max.x; x += chunksSizeInCubes.x)
                {
                    for (int z = startChunkPosition.z; z < max.z; z += chunksSizeInCubes.z)
                    {
                        int localChunkX = x / chunksSizeInCubes.x;
                        int localChunkY = y / chunksSizeInCubes.y;
                        int localChunkZ = z / chunksSizeInCubes.z;
                        long positionHash = PositionLongHasher.GetHashFromPosition(localChunkX, localChunkY, localChunkZ);
                        chunksContainer.TryGetValue(positionHash, out var chunk);
                        
                        if (chunk == null)
                        {
                            continue;
                        }

                        unsafe
                        {
                            affectedPositions.Add(new Vector3Int(localChunkX, localChunkY, localChunkZ));
                            pointers.Add(positionHash, new UnsafeNativeArray<VoxelData>(chunk.ChunkData.RawData, Allocator.Persistent));
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
