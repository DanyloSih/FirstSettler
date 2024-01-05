using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunk
    {
        public IMeshGenerationAlgorithm MeshGenerationAlgorithm { get; }
        public GameObject RootGameObject { get; }
        public Vector3Int ChunkSize { get; }
        public Vector3Int ChunkPosition { get; }
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