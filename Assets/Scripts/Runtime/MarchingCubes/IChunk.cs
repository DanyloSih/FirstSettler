using UnityEngine;

namespace MarchingCubesProject
{
    public interface IChunk
    {
        BasicChunkSettings BasicChunkSettings { get; }
        Vector3Int ChunkPosition { get; }
        MeshData MeshData { get; }
        ChunkData ChunkData { get; }
        ChunkNeighbors Neighbors { get; }

        void InitializeBasicData(
            BasicChunkSettings basicChunkSettings,
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial,
            Vector3Int chunkPosition,
            ChunkData chunkData);

        void InitializeNeighbors(ChunkNeighbors chunkNeighbors);

        void UpdateMesh();
    }
}