using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct ChunkDataPoint
    {
        public ChunkDataPoint(Vector3 globalChunkDataPoint, Vector3 localChunkPosition, Vector3 localChunkDataPoint, float volume, int materialHash)
        {
            GlobalChunkDataPoint = globalChunkDataPoint;
            LocalChunkPosition = localChunkPosition;
            LocalChunkDataPoint = localChunkDataPoint;
            Volume = volume;
            MaterialHash = materialHash;
        }

        public Vector3 GlobalChunkDataPoint { get; set; }
        public Vector3 LocalChunkPosition { get; set; }
        public Vector3 LocalChunkDataPoint { get; set; }
        public float Volume { get; set; }
        public int MaterialHash { get; set; }
    }
}
