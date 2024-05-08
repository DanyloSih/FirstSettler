using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Math;
using Utilities.Shaders;
using Utilities.Threading;
using Utilities.Threading.Extensions;
using World.Data;
using World.Organization;
using Zenject;

namespace MarchingCubesProject
{
    public class GPUChunkDataGenerator : MonoBehaviour, IChunkDataProvider
    {
        [Inject] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;

        [SerializeField] private ComputeShader _generationComputeShader;
        [SerializeField] private float _maxHeight = 256;
        [SerializeField] private float _minHeight;
        [SerializeField] private int _octaves = 4;
        [SerializeField] private float _persistence = 0.5f;
        [SerializeField] private float _frequency = 1.0f;
        [SerializeField] private float _amplitude = 1.0f;
        [SerializeField] private Vector3Int _voxelsOffset;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private ComputeBuffer _heightHashAssociationsBuffer;
        private ComputeBuffer _minMaxAssociations;
        private bool _isInitialized;
        private NativeArray<VoxelData> _voxelsArray;
        private ComputeBuffer _chunkPositionsBuffer;
        private ComputeBuffer _voxelsBuffer;
        private int _voxelsBufferLength;
        private ComputeBuffer _rectPrisms;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        protected void OnEnable()
        {
            InitializeBuffers();
        }

        protected void OnDisable()
        {
            DisposeVoxelsDataBuffer();
            DisposeChunksPositionsBuffer();

            _heightHashAssociationsBuffer.Dispose();
            _minMaxAssociations.Dispose();
            _rectPrisms.Dispose();
        }

        public async Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions,
            Vector3Int chunkOffset,
            Vector3Int chunkDataSize,
            CancellationToken? cancellationToken = null)
        {
            if (!enabled)
            {
                throw new System.InvalidOperationException($"Object with name \"{name}\" disabled!");
            }

            InitializeChunksPositionsBuffer(generatingChunksLocalPositions);

            int chunkDataVolume = chunkDataSize.x * chunkDataSize.y * chunkDataSize.z;

            InitializeBuffers();
            
            int chunksCount = generatingChunksLocalPositions.Length;
            CreateVoxelsDataBuffer(chunkDataVolume * chunksCount);

            var kernelId = _generationComputeShader.FindKernel("GenerateData");
            _rectPrisms.SetData(new RectPrismInt[] {
                new RectPrismInt(chunkDataSize),
                new RectPrismInt(chunkOffset)
            });   

            _generationComputeShader.SetBuffer(kernelId, "ChunkData", _voxelsBuffer);
            _generationComputeShader.SetBuffer(kernelId, "HeightAndHashAssociations", _heightHashAssociationsBuffer);
            _generationComputeShader.SetBuffer(kernelId, "MinMaxAssociations", _minMaxAssociations);
            _generationComputeShader.SetBuffer(kernelId, "ChunkPrisms", _rectPrisms);
            _generationComputeShader.SetBuffer(kernelId, "LocalChunksPositions", _chunkPositionsBuffer);
            _generationComputeShader.SetInt("AssociationsCount", _heightAssociations.Count);
            _generationComputeShader.SetInt("Octaves", _octaves);
            _generationComputeShader.SetFloat("Persistence", _persistence);
            _generationComputeShader.SetFloat("Frequency", _frequency);
            _generationComputeShader.SetFloat("Amplitude", _amplitude);
            _generationComputeShader.SetFloat("MaxHeight", _maxHeight);
            _generationComputeShader.SetFloat("MinHeight", _minHeight);
            _generationComputeShader.Dispatch(
                kernelId, chunksCount, Mathf.CeilToInt(chunkDataVolume / 256f), 1);

            AsyncGPUReadbackRequest request = AsyncGPUReadback.RequestIntoNativeArray(
                ref _voxelsArray, _voxelsBuffer);

            await AsyncUtilities.WaitWhile(() => !request.done, 1, cancellationToken);

            List<ThreedimensionalNativeArray<VoxelData>> result = new();

            if (cancellationToken.IsCanceled())
            {
                return result;
            }

            for (int i = 0; i < chunksCount; i++)
            {
                var subArray = _voxelsArray.GetSubArray(i * chunkDataVolume, chunkDataVolume);
                NativeArray<VoxelData> subarray = new(subArray, Allocator.Persistent);
                result.Add(new ThreedimensionalNativeArray<VoxelData>(subarray, chunkDataSize));
            }

            return result;
        }

        private void InitializeBuffers()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _heightAssociations.Initialize();
                InitializeAssociationsBuffer();
                InitializeMinMaxBuffer();
                InitializeParallelepipedsBuffer();
            }
        }

        private void InitializeChunksPositionsBuffer(NativeArray<Vector3Int> positions)
        {
            int length = positions.Length;

            if (_chunkPositionsBuffer == null)
            {
                CreateEmptyChunksPositionsBuffer(length);
            }
            else if (_chunkPositionsBuffer.count != length)
            {
                DisposeChunksPositionsBuffer();
                CreateEmptyChunksPositionsBuffer(length);
            }

            _chunkPositionsBuffer.SetData(positions);
        }

        private void DisposeChunksPositionsBuffer()
        {
            if (_chunkPositionsBuffer != null)
            {
                _chunkPositionsBuffer.Dispose();
                _chunkPositionsBuffer = null;
            }
        }

        private void CreateEmptyChunksPositionsBuffer(int length)
        {
            _chunkPositionsBuffer = BuffersFactory.CreateCompute(length, typeof(Vector3Int));
        }

        private void CreateVoxelsDataBuffer(int length)
        {
            if (_voxelsBuffer == null)
            {
                _voxelsArray = new NativeArray<VoxelData>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _voxelsBuffer = BuffersFactory.CreateCompute(length, typeof(VoxelData));
            }
            else if (_voxelsBufferLength != length)
            {
                _voxelsArray.Dispose();
                _voxelsBuffer.Dispose();
                _voxelsBuffer = null;
                _voxelsArray = new NativeArray<VoxelData>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _voxelsBuffer = BuffersFactory.CreateCompute(length, typeof(VoxelData));
            }

            _voxelsBufferLength = length;
        }

        private void DisposeVoxelsDataBuffer()
        {
            if (_voxelsBuffer != null)
            {
                _voxelsBuffer.Dispose();
                _voxelsBuffer = null;
            }

            if (_voxelsArray != null && _voxelsArray.IsCreated)
            {
                _voxelsArray.Dispose();
            }
        }

        private void InitializeAssociationsBuffer()
        {
            _heightHashAssociationsBuffer = BuffersFactory.CreateCompute(
                            _heightAssociations.Count, typeof(HeightAndMaterialHashAssociation));

            var heightAndMaterialHashAssociations = _heightAssociations.GetEnumerable()
                .Select(x => new HeightAndMaterialHashAssociation(x.Height, x.MaterialKey.GetHashCode()))
                .ToArray();

            _heightHashAssociationsBuffer.SetData(heightAndMaterialHashAssociations);
        }

        private void InitializeMinMaxBuffer()
        {
            _minMaxAssociations = BuffersFactory.CreateCompute(
                            2, typeof(HeightAndMaterialHashAssociation));

            _minMaxAssociations.SetData(new KeyValuePair<float, int>?[] {
                _heightAssociations.MinAssociation,
                _heightAssociations.MaxAssociation
            }.Select(x => new HeightAndMaterialHashAssociation(x.Value.Key, x.Value.Value)).ToArray());
        }

        private void InitializeParallelepipedsBuffer()
        {
            _rectPrisms = BuffersFactory.CreateCompute(2, typeof(RectPrismInt));
        }
    }
}
