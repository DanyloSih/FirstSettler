using System.Linq;
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

        private void Awake()
        {
            //int mat = _materialAssociations.GetMaterialKeyHashes().ElementAt(0);
            //var kernelId = _generationComputeShader.FindKernel("CSMain");
            //var voxels = Voxels;
            //int dataSize = sizeof(int) + sizeof(float);
            //_voxelsBuffer = new ComputeBuffer(dataSize, _voxels.FullLength);
            //_voxelsBuffer.SetData(_voxels.RawData);

            //_generationComputeShader.SetBuffer(kernelId, "Result", _voxelsBuffer);
            //_generationComputeShader.SetFloat("Width", _voxels.Width);
            //_generationComputeShader.SetFloat("Height", _voxels.Height);
            //_generationComputeShader.SetFloat("Depth", _voxels.Depth);
            //_generationComputeShader.SetInt("MaterialHash", mat);
            //_generationComputeShader.Dispatch(kernelId, 1024 / 8, 1024 / 8, 1024 / 8);
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

            for (int y = 0; y < height; y++)
            {
                float v = y / (height - 1.0f);
                float globalY = y + yOffset * (height - 1.0f);
                var hash = _heightAssociations.GetMaterialKeyHashByHeight(globalY);

                for (int x = 0; x < width; x++)
                {
                    float u = x / (width - 1.0f);
                    float globalX = x + xOffset * (width - 1.0f);

                    for (int z = 0; z < depth; z++)
                    {
                        float w = z / (depth - 1.0f);
                        float globalZ = z + zOffset * (depth - 1.0f);

                        var heightThreshold = (1 + _fractal.Sample2D(u + xOffset, w + zOffset)) / 2 * _maxHeight + _minHeight;
                        float currentVolume = globalY < heightThreshold ? 1 : 0;
                        chunkData.SetVoxelData(x, y, z, new VoxelData(currentVolume, hash));
                    }
                }
            }

            return chunkData;
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
    }
}
