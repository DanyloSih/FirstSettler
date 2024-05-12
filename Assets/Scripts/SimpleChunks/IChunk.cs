using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks
{
    public interface IChunk
    {
        public GameObject RootGameObject { get; }
        /// <summary>
        /// Coordinates of the chunk relative to other chunks.<br/>
        /// For example: <br/>
        /// There is a chunk with local coordinates (4, 2, 5), <br/>
        /// its neighbor on the left will be a chunk with local coordinates (3, 2, 5), 
        /// <br/>and on the right (5, 2, 5)
        /// </summary>
        public Vector3Int LocalPosition { get; }
        public ThreedimensionalNativeArray<VoxelData> ChunkData { get; }

        public void InitializeBasicData(
            Vector3Int chunkPosition,
            ThreedimensionalNativeArray<VoxelData> chunkData);

        public void ApplyMeshData(MeshData meshData);
    }
}