using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunk
    {
        public GameObject RootGameObject { get; }
        public Vector3Int ChunkSize { get; }
        public Vector3Int ChunkPosition { get; }
        public ChunkData ChunkData { get; }
        public ChunkNeighbors Neighbors { get; }

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings,
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial,
            Vector3Int chunkPosition,
            ChunkData chunkData);

        public void InitializeNeighbors(ChunkNeighbors chunkNeighbors);

        public void UpdateMesh();
    }
}