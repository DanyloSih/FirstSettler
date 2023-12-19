namespace World.Data
{
    public interface IMeshGenerationAlgorithm
    {
        GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get; }

        public void GenerateMeshData(ChunkData chunkData, MeshDataBuffer cashedMeshData);
    }
}