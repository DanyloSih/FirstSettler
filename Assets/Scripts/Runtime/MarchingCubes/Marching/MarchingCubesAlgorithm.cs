using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject
{
    public class MarchingCubesAlgorithm : MarchingAlgorithm
    {
        private ComputeShader _meshGenerationComputeShader;
        private MeshDataBuffersReader _meshDataBuffersReader;

        public MarchingCubesAlgorithm(
			GenerationAlgorithmInfo generationAlgorithmInfo, 
			ComputeShader meshGenerationComputeShader,
            Vector3Int chunkSize,
            float surface)
            : base(generationAlgorithmInfo, surface)
        {
            _meshGenerationComputeShader = meshGenerationComputeShader;
            var maxVerticesCount = chunkSize.x * chunkSize.y * chunkSize.z * generationAlgorithmInfo.MaxVerticesPerMarch;
            _meshDataBuffersReader = new MeshDataBuffersReader(maxVerticesCount);
        }

        public override void Dispose()
        {
            _meshDataBuffersReader.Dispose();
        }

        public override async Task<MeshDataBuffer> GenerateMeshData(ChunkData chunkData)
        {
            ThreedimensionalNativeArray<VoxelData> voxels = chunkData.VoxelsData;
            ComputeBuffer voxelsBuffer = chunkData.GetOrCreateVoxelsDataBuffer();
            MeshBuffers meshBuffers = _meshDataBuffersReader.CreateNewMeshBuffers();
            meshBuffers.ResetCounters();
            
            int kernelId = _meshGenerationComputeShader.FindKernel("CSMain");
            _meshGenerationComputeShader.SetBuffer(kernelId, "ChunkData", voxelsBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "Triangles", meshBuffers.TrianglesBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "Vertices", meshBuffers.VerticesBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "UVs", meshBuffers.UVBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "PolygonsCounter", meshBuffers.PolygonsCounter);
            _meshGenerationComputeShader.SetInt("MaxVericesCount", MeshGenerationAlgorithmInfo.MaxVerticesPerMarch);
            _meshGenerationComputeShader.SetInt("ChunkWidth", voxels.Width);
            _meshGenerationComputeShader.SetInt("ChunkHeight", voxels.Height);
            _meshGenerationComputeShader.SetInt("ChunkDepth", voxels.Depth);
            _meshGenerationComputeShader.SetFloat("Surface", Surface);
            _meshGenerationComputeShader.Dispatch(
                kernelId, voxels.Width - 1, voxels.Height - 1, voxels.Depth - 1);

            _meshDataBuffersReader.UpdatePolygonsCount(meshBuffers);
            var result = await _meshDataBuffersReader.ReadFromBuffersToMeshData(meshBuffers);
            meshBuffers.DisposeAllBuffers();
            return result;
        }
    }
}
