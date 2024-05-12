using SimpleChunks.DataGeneration;
using UnityEngine;

namespace SimpleChunks.Tools
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct ChunkPoint
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
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
