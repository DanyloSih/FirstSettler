using SimpleChunks.DataGeneration;
using UnityEngine;

namespace SimpleChunks.Tools
{
    public struct ChunkPointWithData
    {
        public Vector3Int LocalChunkPosition;
        public Vector3Int LocalVoxelPosition;
        public VoxelData VoxelData;
        public bool IsInitialized;

        public ChunkPointWithData(
            Vector3Int localChunkPosition,
            Vector3Int localVoxelPosition,
            VoxelData voxelData)
        {
            LocalChunkPosition = localChunkPosition;
            LocalVoxelPosition = localVoxelPosition;
            VoxelData = voxelData;
            IsInitialized = true;
        }

        public ChunkPointWithData(
           ChunkPoint chunkPoint,
           VoxelData voxelData)
        {
            LocalChunkPosition = chunkPoint.LocalChunkPosition;
            LocalVoxelPosition = chunkPoint.LocalVoxelPosition;
            VoxelData = voxelData;
            IsInitialized = true;
        }
    }
}
