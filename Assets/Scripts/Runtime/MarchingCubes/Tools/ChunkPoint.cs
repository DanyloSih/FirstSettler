using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct ChunkPoint
    {
        public Vector3 LocalChunkPosition;
        public Vector3 LocalChunkDataPoint;
        public float Volume;
        public int MaterialHash;

        public ChunkPoint(
            Vector3 localChunkPosition,
            Vector3 localChunkDataPoint,
            float volume,
            int materialHash)
        {
            LocalChunkPosition = localChunkPosition;
            LocalChunkDataPoint = localChunkDataPoint;
            Volume = volume;
            MaterialHash = materialHash;
        }

        public static bool operator ==(ChunkPoint left, ChunkPoint right)
        {
            return left.LocalChunkPosition == right.LocalChunkPosition 
                && left.LocalChunkDataPoint == right.LocalChunkDataPoint
                && left.Volume == right.Volume
                && left.MaterialHash == right.MaterialHash;
        }

        public static bool operator !=(ChunkPoint left, ChunkPoint right)
        {
            return !(left == right);
        }
    }
}
