using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct AffectedNeighborData
    {
        public Vector3Int AffectedLocalChunkPosition;
        public Vector3Int AffectedLocalChunkDataPoint;

        public AffectedNeighborData(
            Vector3Int affectedLocalChunkPosition,
            Vector3Int affectedLocalChunkDataPoint)
        {
            AffectedLocalChunkPosition = affectedLocalChunkPosition;
            AffectedLocalChunkDataPoint = affectedLocalChunkDataPoint;
        }
    }
}
