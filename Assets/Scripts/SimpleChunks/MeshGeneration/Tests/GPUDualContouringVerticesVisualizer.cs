using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Math.Extensions;
using Utilities.Shaders;
using Zenject;
using Utilities.Shaders.Extensions;
using UnityEngine.Rendering;
using System;
using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;

namespace MeshGeneration.Tests
{
    public class GPUDualContouringVerticesVisualizer : MonoBehaviour
    {
        private List<GameObject> _pointers = new List<GameObject>();

        [Inject] ChunkPrismsProvider _chunkPrismsProvider;

        [SerializeField] private Transform _anchor;
        [SerializeField] private GameObject _pointerPrefab;
        [SerializeField] private int _generationAreaEdgeLength = 2;
        [SerializeField] private ChunkDataGenerator _chunkDataGenerator;
        [SerializeField] private ComputeShader _verticesGenerationShader;

        private NativeArrayManager<Vector3Int> _positionsArrayManager = new NativeArrayManager<Vector3Int>(
           (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

        private NativeArrayManager<Vector3> _verticesArrayManager = new NativeArrayManager<Vector3>(
          (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

        private NativeArrayManager<VertexInfo> _verticesInfoArrayManager = new NativeArrayManager<VertexInfo>(
          (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

        private ComputeBufferManager _chunkDataBufferManager = new ComputeBufferManager(
            (count) => BuffersFactory.CreateCompute(count, typeof(VoxelData)));

        private ComputeBufferManager _chunkPrismsBufferManager = new ComputeBufferManager(
            (count) => BuffersFactory.CreateCompute(count, typeof(RectPrismInt)));

        private ComputeBufferManager _verticesBufferManager = new ComputeBufferManager(
            (count) => BuffersFactory.CreateCompute(count, typeof(Vector3)));

        private ComputeBufferManager _verticesInfoBufferManager = new ComputeBufferManager(
            (count) => BuffersFactory.CreateCompute(count, typeof(VertexInfo)));

        protected void OnDestroy()
        {
            _positionsArrayManager.Dispose();
            _verticesArrayManager.Dispose();
            _verticesInfoArrayManager.Dispose();

            _chunkDataBufferManager.Dispose();
            _chunkPrismsBufferManager.Dispose();
            _verticesBufferManager.Dispose();
            _verticesInfoBufferManager.Dispose();
        }

        [Button]
        public async void RegenerateVertices()
        {
            ShapeIntArea<RectPrismInt> generationArea = new ShapeIntArea<RectPrismInt>(
                new RectPrismInt(Vector3Int.one * _generationAreaEdgeLength), Vector3Int.FloorToInt(_anchor.position));

            NativeArray<Vector3Int> positionsArray = _positionsArrayManager
                .GetObjectInstance(generationArea.Shape.Volume);
            generationArea.FillArrayWithPositions(positionsArray);

            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> data 
                = await _chunkDataGenerator.GenerateChunksRawData(positionsArray);

            int chunksCount = positionsArray.Length;

            int voxelsCount = chunksCount * _chunkPrismsProvider.VoxelsPrism.Volume;
            int cubesCount = chunksCount * _chunkPrismsProvider.CubesPrism.Volume;

            ComputeBuffer chunksDataBuffer = _chunkDataBufferManager.GetObjectInstance(voxelsCount)
                .FillBufferWithChunksData(positionsArray, data.AsReadOnly());

            ComputeBuffer chunkPrismBuffer = _chunkPrismsBufferManager.GetObjectInstance(2);
            chunkPrismBuffer.SetData(_chunkPrismsProvider.PrismsArray);

            ComputeBuffer verticesBuffer = _verticesBufferManager.GetObjectInstance(cubesCount);
            ComputeBuffer verticesInfoBuffer = _verticesInfoBufferManager.GetObjectInstance(cubesCount);
            _verticesArrayManager.Dispose();
            _verticesInfoArrayManager.Dispose();
            var verticesArray = _verticesArrayManager.GetObjectInstance(cubesCount);
            var verticesInfoArray = _verticesInfoArrayManager.GetObjectInstance(cubesCount);
            verticesBuffer.SetData(verticesArray);
            verticesInfoBuffer.SetData(verticesInfoArray);

            int kernelId = _verticesGenerationShader.FindKernel("GenerateMesh");

            _verticesGenerationShader.SetBuffer(kernelId, "ChunkData", chunksDataBuffer);
            _verticesGenerationShader.SetBuffer(kernelId, "ChunkPrisms", chunkPrismBuffer);
            _verticesGenerationShader.SetBuffer(kernelId, "Vertices", verticesBuffer);
            _verticesGenerationShader.SetBuffer(kernelId, "VerticesInfo", verticesInfoBuffer);
            _verticesGenerationShader.DispatchConsideringGroupSizes(kernelId, positionsArray.Length, cubesCount, 1);

            var requests = new AsyncGPUReadbackRequest[] {
                AsyncGPUReadback.RequestIntoNativeArray(ref verticesArray, verticesBuffer),
                AsyncGPUReadback.RequestIntoNativeArray(ref verticesInfoArray, verticesInfoBuffer)
            };

            await requests.WaitUntilDone();

            if (requests.HasError())
            {
                throw new InvalidOperationException();
            }

            foreach (GameObject obj in _pointers) 
            {
                Destroy(obj);
            }
            _pointers.Clear();

            for (int i = 0; i < verticesArray.Length; i++)
            {
                _pointers.Add(Instantiate(_pointerPrefab, verticesArray[i], Quaternion.identity));
            }
        }
    }
}