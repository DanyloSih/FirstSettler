using System;
using System.Threading.Tasks;

namespace World.Data
{
    public interface IMeshGenerationAlgorithm : IDisposable
    {
        GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get; }

        public Task<MeshDataBuffer> GenerateMeshData(ChunkData chunkData);
    }
}