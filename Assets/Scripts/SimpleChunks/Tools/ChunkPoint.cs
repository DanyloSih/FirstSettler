using UnityEngine;

namespace SimpleChunks.Tools
{
    public struct ChunkPoint
    {
        public Vector3Int LocalChunkPosition;
        public Vector3Int LocalVoxelPosition;

        public ChunkPoint(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            LocalChunkPosition = localChunkPosition;
            LocalVoxelPosition = localChunkDataPoint;
        }
    }
}
