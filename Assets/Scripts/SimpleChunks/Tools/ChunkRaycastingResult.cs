using UnityEngine;

namespace SimpleChunks.Tools
{
    public struct ChunkRaycastingResult
    {
        public bool IsChunkHit;
        public IChunk Chunk;
        public Ray Ray;
        public RaycastHit Hit;
        public Vector3 GlobalChunkDataPoint;
        public Vector3 LocalChunkDataPoint;
        public float Scale;
    }
}
