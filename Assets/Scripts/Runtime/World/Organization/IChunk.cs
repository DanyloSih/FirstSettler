using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunk
    {
        public IMeshGenerationAlgorithm MeshGenerationAlgorithm { get; }
        public GameObject RootGameObject { get; }
        /// <summary>
        /// Coordinates of the chunk relative to other chunks.<br/>
        /// For example: <br/>
        /// There is a chunk with local coordinates (4, 2, 5), <br/>
        /// its neighbor on the left will be a chunk with local coordinates (3, 2, 5), 
        /// <br/>and on the right (5, 2, 5)
        /// </summary>
        public Vector3Int LocalPosition { get; }
        public ChunkData ChunkData { get; }

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings,
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial,
            Vector3Int chunkPosition,
            ChunkData chunkData,
            MeshDataBuffersKeeper meshDataBuffer);

        public Task UpdateMesh();
    }
}