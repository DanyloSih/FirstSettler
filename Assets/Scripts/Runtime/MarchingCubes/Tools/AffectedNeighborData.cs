using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct AffectedNeighborData
    {
        public Vector3Int AffectMask;
        public Vector3Int AffectedLocalChunkPosition;
        public Vector3Int AffectedLocalChunkDataPoint;

        public AffectedNeighborData(
            Vector3Int affectMask,
            Vector3Int affectedLocalChunkPosition,
            Vector3Int affectedLocalChunkDataPoint)
        {
            AffectMask = affectMask;
            AffectedLocalChunkPosition = affectedLocalChunkPosition;
            AffectedLocalChunkDataPoint = affectedLocalChunkDataPoint;
        }
    }
}
