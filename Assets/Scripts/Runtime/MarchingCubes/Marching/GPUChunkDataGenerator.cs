using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirstSettler.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Math;
using Utilities.Threading;
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
        [SerializeField] private Vector3Int _voxelsOffset;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private ComputeBuffer _heightHashAssociationsBuffer;
        private ComputeBuffer _minMaxAssociations;
        private bool _isInitialized;
        private NativeArray<VoxelData> _voxelsArray;
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
            if (_voxelsBuffer != null && _voxelsBuffer.IsValid())
            {
                _voxelsBuffer.Dispose();
                _voxelsBuffer = null;
            }

            if (_voxelsArray.IsCreated)
            {
                _voxelsArray.Dispose();
            }

            _heightHashAssociationsBuffer.Dispose();
            _minMaxAssociations.Dispose();
            _rectPrisms.Dispose();
        }

        public async Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            RectPrismInt loadingArea, Vector3Int anchor, Vector3Int chunkOffset, Vector3Int chunkDataSize)
        {
            if (!enabled)
            {
                throw new System.InvalidOperationException($"Object with name \"{name}\" disabled!");
            }

            int chunkDataVolume = chunkDataSize.x * chunkDataSize.y * chunkDataSize.z;
            Vector3Int globalStartPoint = anchor * chunkOffset;
            Vector3Int globalSize = loadingArea.Size * chunkDataSize;
            int globalVolume = globalSize.x * globalSize.y * globalSize.z;

            InitializeBuffers();
            var kernelId = _generationComputeShader.FindKernel("CSMain");
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);
            _rectPrisms.SetData(new RectPrismInt[] {
                new RectPrismInt(chunkDataSize),
                new RectPrismInt(chunkOffset),
                new RectPrismInt(globalSize),
                loadingArea
            });

            _ = GetOrCreateVoxelsDataBuffer(globalVolume);

            _generationComputeShader.SetBuffer(kernelId, "ChunkData", _voxelsBuffer);
            _generationComputeShader.SetBuffer(kernelId, "HeightAndHashAssociations", _heightHashAssociationsBuffer);
            _generationComputeShader.SetBuffer(kernelId, "MinMaxAssociations", _minMaxAssociations);
            _generationComputeShader.SetBuffer(kernelId, "ChunkSizeAndChunkOffsetAndGlobalAndLoadingAreaRectPrisms", _rectPrisms);
            _generationComputeShader.SetInt("ChunkGlobalPositionX", _voxelsOffset.x + globalStartPoint.x);
            _generationComputeShader.SetInt("ChunkGlobalPositionY", _voxelsOffset.y + globalStartPoint.y);
            _generationComputeShader.SetInt("ChunkGlobalPositionZ", _voxelsOffset.z + globalStartPoint.z);
            _generationComputeShader.SetInt("AssociationsCount", _heightAssociations.Count);
            _generationComputeShader.SetInt("Octaves", _octaves);
            _generationComputeShader.SetFloat("Persistence", _persistence);
            _generationComputeShader.SetFloat("Frequency", _frequency);
            _generationComputeShader.SetFloat("Amplitude", _amplitude);
            _generationComputeShader.SetFloat("MaxHeight", _maxHeight);
            _generationComputeShader.SetFloat("MinHeight", _minHeight);
            _generationComputeShader.Dispatch(
                kernelId, globalSize.x, globalSize.y, globalSize.z);

            AsyncGPUReadbackRequest request = AsyncGPUReadback.RequestIntoNativeArray(
                ref _voxelsArray, _voxelsBuffer);

            await AsyncUtilities.WaitWhile(() => !request.done, 1);

            List<ThreedimensionalNativeArray<VoxelData>> result
                = new List<ThreedimensionalNativeArray<VoxelData>>();


            for (int i = 0; i < loadingArea.Volume; i++)
            {
                NativeArray<VoxelData> subarray = new NativeArray<VoxelData>(
                    _voxelsArray.GetSubArray(i * chunkDataVolume, chunkDataVolume), Allocator.Persistent);

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

        private (ComputeBuffer, NativeArray<VoxelData>) GetOrCreateVoxelsDataBuffer(int length)
        {
            if (_voxelsBuffer == null)
            {
                _voxelsArray = new NativeArray<VoxelData>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _voxelsBuffer = ComputeBufferExtensions.Create(length, typeof(VoxelData));
            }
            else if (_voxelsBufferLength != length)
            {
                _voxelsArray.Dispose();
                _voxelsBuffer.Dispose();
                _voxelsBuffer = null;
                _voxelsArray = new NativeArray<VoxelData>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _voxelsBuffer = ComputeBufferExtensions.Create(length, typeof(VoxelData));
            }

            _voxelsBufferLength = length;

            return (_voxelsBuffer, _voxelsArray);
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

        private void InitializeParallelepipedsBuffer()
        {
            _rectPrisms = ComputeBufferExtensions.Create(4, typeof(RectPrismInt));
        }
    }
}
