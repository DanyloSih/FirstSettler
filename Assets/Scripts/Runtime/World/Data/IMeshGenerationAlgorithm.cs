using System.Threading.Tasks;

namespace World.Data
{
    public interface IMeshGenerationAlgorithm
    {
        GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get; }

        public Task<DisposableMeshData> GenerateMeshData(ChunkData chunkData);
    }
}