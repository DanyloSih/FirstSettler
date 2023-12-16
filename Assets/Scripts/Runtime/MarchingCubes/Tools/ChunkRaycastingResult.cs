using UnityEngine;
using World.Organization;

namespace MarchingCubesProject.Tools
{
    public struct ChunkRaycastingResult
    {
        public bool IsChunkHited;
        public IChunk Chunk;
        public Ray Ray;
        public RaycastHit Hit;
        public Vector3 GlobalChunkDataPoint;
        public Vector3 LocalChunkDataPoint;
        public float Scale;
    }
}
