using System;
using System.Threading.Tasks;

namespace World.Data
{
    public interface IMeshGenerationAlgorithm : IDisposable
    {
        GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get; }

        public Task<MeshData> GenerateMeshData(ChunkData chunkData);
    }
}