using System.Threading.Tasks;

namespace World.Data
{
    public interface IMeshGenerationAlgorithm
    {
        GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get; }

        public Task GenerateMeshData(ChunkData chunkData, MeshDataBuffersKeeper cashedMeshData);
    }
}