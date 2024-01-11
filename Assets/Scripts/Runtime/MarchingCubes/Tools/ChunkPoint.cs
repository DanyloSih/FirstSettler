using UnityEngine;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public struct ChunkPoint
    {
        public Vector3 LocalChunkPosition;
        public Vector3 LocalChunkDataPoint;
        public VoxelData VoxelData;
        public bool IsInitialized;

        public ChunkPoint(
            Vector3 localChunkPosition,
            Vector3 localChunkDataPoint,
            VoxelData voxelData)
        {
            LocalChunkPosition = localChunkPosition;
            LocalChunkDataPoint = localChunkDataPoint;
            VoxelData = voxelData;
            IsInitialized = true;
        }

        public static bool operator ==(ChunkPoint left, ChunkPoint right)
        {
            return left.LocalChunkPosition == right.LocalChunkPosition 
                && left.LocalChunkDataPoint == right.LocalChunkDataPoint
                && left.VoxelData == right.VoxelData;
        }

        public static bool operator !=(ChunkPoint left, ChunkPoint right)
        {
            return !(left == right);
        }
    }
}
