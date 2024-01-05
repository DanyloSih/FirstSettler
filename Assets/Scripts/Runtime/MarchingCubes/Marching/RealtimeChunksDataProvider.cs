using System.Collections.Generic;
using System.Threading.Tasks;
using ProceduralNoiseProject;
using UnityEngine;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public class RealtimeChunksDataProvider : MonoBehaviour, IChunksDataProvider
    {
        [SerializeField] private ComputeShader _generationComputeShader;
        [SerializeField] private float _maxHeight = 256;
        [SerializeField] private float _minHeight;
        [SerializeField] private int _octaves;
        [SerializeField] private float _frequency;
        [SerializeField] private int _seed = 0;
        [SerializeField] private Vector3Int _chunksOffset;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private FractalNoise _fractal;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        public async Task<ChunkData> GetChunkData(int x, int y, int z, Vector3Int chunkSize)
        {
            Vector3Int chunkDataSize = chunkSize + new Vector3Int(1, 1, 1) * 1;
            var voxels = new MultidimensionalArray<VoxelData>(chunkDataSize);

            await FillVoxelsArray(voxels, x, y, z);
            return new ChunkData(voxels);
        }

        private Task FillVoxelsArray(MultidimensionalArray<VoxelData> voxels, int x, int y, int z)
        {
            var kernelId = _generationComputeShader.FindKernel("CSMain");          
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);

            var chunkDataBuffer = voxels.GetOrCreateVoxelsDataBuffer();

            _generationComputeShader.SetBuffer(kernelId, "ChunkData", chunkDataBuffer);
            _generationComputeShader.SetInt("MatHash", mat);
            _generationComputeShader.SetInt("ChunkWidth", voxels.Width);
            _generationComputeShader.SetInt("ChunkHeight", voxels.Height);
            _generationComputeShader.SetInt("ChunkDepth", voxels.Depth);
            _generationComputeShader.SetInt("ChunkGlobalPositionX", x * (voxels.Width - 1));
            _generationComputeShader.SetInt("ChunkGlobalPositionY", y * (voxels.Height - 1));
            _generationComputeShader.SetInt("ChunkGlobalPositionZ", z * (voxels.Depth - 1));
            _generationComputeShader.SetFloat("MinHeight", _minHeight);
            _generationComputeShader.Dispatch(
                kernelId, voxels.Width, voxels.Height, voxels.Depth);

            voxels.GetDataFromVoxelsBuffer(chunkDataBuffer);
            return Task.CompletedTask;
        }
    }
}
