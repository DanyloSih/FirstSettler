using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstSettler.Extensions;
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
        [SerializeField] private int _octaves = 4;
        [SerializeField] private float _persistence = 0.5f;
        [SerializeField] private float _frequency = 1.0f;
        [SerializeField] private float _amplitude = 1.0f;
        [SerializeField] private int _seed = 0;
        [SerializeField] private Vector3Int _voxelsOffset;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private FractalNoise _fractal;
        private ComputeBuffer _heightHashAssociationsBuffer;
        private ComputeBuffer _minMaxAssociations;
        private bool _isInitialized;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        protected void OnEnable()
        {
            InitializeBuffers();
        }

        protected void OnDisable()
        {
            _heightHashAssociationsBuffer.Dispose();
            _minMaxAssociations.Dispose();
        }

        public async Task FillChunkData(ChunkData chunkData, Vector3Int chunkGlobalPosition)
        {
            if (!enabled)
            {
                throw new System.InvalidOperationException($"Object with name \"{name}\" disabled!");
            }

            InitializeBuffers();
            var kernelId = _generationComputeShader.FindKernel("CSMain");
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);
            var voxels = chunkData.VoxelsData;
            var chunkDataBuffer = chunkData.GetOrCreateVoxelsDataBuffer();

            _generationComputeShader.SetBuffer(kernelId, "ChunkData", chunkDataBuffer);
            _generationComputeShader.SetBuffer(kernelId, "HeightAndHashAssociations", _heightHashAssociationsBuffer);
            _generationComputeShader.SetBuffer(kernelId, "MinMaxAssociations", _minMaxAssociations);
            _generationComputeShader.SetInt("MatHash", mat);
            _generationComputeShader.SetInt("ChunkWidth", voxels.Width);
            _generationComputeShader.SetInt("ChunkHeight", voxels.Height);
            _generationComputeShader.SetInt("ChunkDepth", voxels.Depth);
            _generationComputeShader.SetInt("ChunkGlobalPositionX", _voxelsOffset.x + chunkGlobalPosition.x);
            _generationComputeShader.SetInt("ChunkGlobalPositionY", _voxelsOffset.y + chunkGlobalPosition.y);
            _generationComputeShader.SetInt("ChunkGlobalPositionZ", _voxelsOffset.z + chunkGlobalPosition.z);
            _generationComputeShader.SetInt("AssociationsCount", _heightAssociations.Count);
            _generationComputeShader.SetInt("Octaves", _octaves);
            _generationComputeShader.SetFloat("Persistence", _persistence);
            _generationComputeShader.SetFloat("Frequency", _frequency);
            _generationComputeShader.SetFloat("Amplitude", _amplitude);
            _generationComputeShader.SetFloat("MaxHeight", _maxHeight);
            _generationComputeShader.SetFloat("MinHeight", _minHeight);
            _generationComputeShader.Dispatch(
                kernelId, voxels.Width, voxels.Height, voxels.Depth);

            await chunkData.GetDataFromVoxelsBuffer(chunkDataBuffer);
        }

        private void InitializeBuffers()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _heightAssociations.Initialize();
                InitializeAssociationsBuffer();
                InitializeMinMaxBuffer();
            }
        }

        private void InitializeAssociationsBuffer()
        {
            _heightHashAssociationsBuffer = ComputeBufferExtensions.Create(
                            _heightAssociations.Count, typeof(HeightAndMaterialHashAssociation));

            var heightAndMaterialHashAssociations = _heightAssociations.GetEnumerable()
                .Select(x => new HeightAndMaterialHashAssociation(x.Height, x.MaterialKey.GetHashCode()))
                .ToArray();

            _heightHashAssociationsBuffer.SetData(heightAndMaterialHashAssociations);
        }

        private void InitializeMinMaxBuffer()
        {
            _minMaxAssociations = ComputeBufferExtensions.Create(
                            2, typeof(HeightAndMaterialHashAssociation));

            _minMaxAssociations.SetData(new KeyValuePair<float, int>?[] {
                _heightAssociations.MinAssociation,
                _heightAssociations.MaxAssociation
            }.Select(x => new HeightAndMaterialHashAssociation(x.Value.Key, x.Value.Value)).ToArray());
        }
    }
}
