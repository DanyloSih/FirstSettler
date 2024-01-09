using System.Threading.Tasks;
using ProceduralNoiseProject;
using UnityEngine;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public class GPUChunkDataGenerator : MonoBehaviour, IChunkDataProvider
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

        public async Task FillChunkData(ChunkData chunkData, int chunkLocalX, int chunkLocalY, int chunkLocalZ)
        {
            var kernelId = _generationComputeShader.FindKernel("CSMain");
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);
            var voxels = chunkData.VoxelsData;
            var chunkDataBuffer = chunkData.GetOrCreateVoxelsDataBuffer();

            _generationComputeShader.SetBuffer(kernelId, "ChunkData", chunkDataBuffer);
            _generationComputeShader.SetInt("MatHash", mat);
            _generationComputeShader.SetInt("ChunkWidth", voxels.Width);
            _generationComputeShader.SetInt("ChunkHeight", voxels.Height);
            _generationComputeShader.SetInt("ChunkDepth", voxels.Depth);
            _generationComputeShader.SetInt("ChunkGlobalPositionX", chunkLocalX * (voxels.Width - 1));
            _generationComputeShader.SetInt("ChunkGlobalPositionY", chunkLocalY * (voxels.Height - 1));
            _generationComputeShader.SetInt("ChunkGlobalPositionZ", chunkLocalZ * (voxels.Depth - 1));
            _generationComputeShader.SetFloat("MinHeight", _minHeight);
            _generationComputeShader.Dispatch(
                kernelId, voxels.Width, voxels.Height, voxels.Depth);

            await chunkData.GetDataFromVoxelsBuffer(chunkDataBuffer);
        }
    }
}
