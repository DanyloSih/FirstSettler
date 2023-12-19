using System.Linq;
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

        private MultidimensionalArray<VoxelData> _voxels;
        private ComputeBuffer _voxelsBuffer;
        private FractalNoise _fractal;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        public MultidimensionalArray<VoxelData> Voxels
            => _voxels = _voxels ?? new MultidimensionalArray<VoxelData>(512, 512, 512);

        public Task LoadRegion()
        {
            var kernelId = _generationComputeShader.FindKernel("CSMain");
            var voxels = Voxels;
            int dataSize = sizeof(int) + sizeof(float);
            _voxelsBuffer = new ComputeBuffer(voxels.FullLength, dataSize);
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);
            _voxelsBuffer.SetData(voxels.RawData);

            _generationComputeShader.SetInt("MatHash", mat);
            _generationComputeShader.SetBuffer(kernelId, "Result", _voxelsBuffer);
            _generationComputeShader.SetInt("RegionWidth", voxels.Width);
            _generationComputeShader.SetInt("RegionHeight", voxels.Height);
            _generationComputeShader.SetInt("RegionDepth", voxels.Depth);
            _generationComputeShader.SetInt("RegionPositionX", -voxels.Width / 2);
            _generationComputeShader.SetInt("RegionPositionY", -voxels.Height / 2);
            _generationComputeShader.SetInt("RegionPositionZ", -voxels.Depth / 2);
            _generationComputeShader.Dispatch(kernelId, voxels.Width / 8, voxels.Height / 8, voxels.Depth / 8);
            _voxelsBuffer.GetData(voxels.RawData);
            _voxelsBuffer.Dispose();
            return Task.CompletedTask;
        }

        public ChunkData GetChunkData(int x, int y, int z, Vector3Int chunkDataSize)
        {
            UpdateNoise();

            return UpdateVoxelArray(
                _chunksOffset.x + x,
                _chunksOffset.y + y,
                _chunksOffset.z + z,
                chunkDataSize);
        }

        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(_seed, _frequency);
            _fractal = new FractalNoise(perlin, _octaves, _frequency);
        }

        private ChunkData UpdateVoxelArray(float xOffset, float yOffset, float zOffset, Vector3Int chunkDataSize)
        {
            MultidimensionalArray<VoxelData> voxels = Voxels;

            int width = chunkDataSize.x;
            int height = chunkDataSize.y;
            int depth = chunkDataSize.z;

            Vector3Int offset = new Vector3Int(
                Mathf.FloorToInt(xOffset * (chunkDataSize.x - 1.0f)), 
                Mathf.FloorToInt(yOffset * (chunkDataSize.y - 1.0f)),
                Mathf.FloorToInt(zOffset * (chunkDataSize.z - 1.0f)));
            offset = voxels.Size / 2 + offset;

            var chunkData = new ChunkData(
                new MultidimensionalArrayRegion<VoxelData>(
                    voxels, chunkDataSize, offset));

            //var hash = _heightAssociations.GetMaterialKeyHashByHeight(0);
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        for (int z = 0; z < depth; z++)
            //        {
            //            chunkData.SetVoxelData(x, y, z, new VoxelData(1, hash));
            //        }
            //    }
            //}

            return chunkData;
        }
    }
}
