using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct VoxelBlueprint
    {
        public Vector3 GlobalChunkDataPoint;
        public Vector3 LocalChunkPosition;
        public Vector3 LocalChunkDataPoint;
        public float Volume;
        public int MaterialHash;

        public VoxelBlueprint(
            Vector3 globalChunkDataPoint,
            Vector3 localChunkPosition,
            Vector3 localChunkDataPoint,
            float volume,
            int materialHash)
        {
            GlobalChunkDataPoint = globalChunkDataPoint;
            LocalChunkPosition = localChunkPosition;
            LocalChunkDataPoint = localChunkDataPoint;
            Volume = volume;
            MaterialHash = materialHash;
        }
    }
}
