using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Shaders;
using Utilities.Shaders.Extensions;
using Utilities.Threading;
using Utilities.Threading.Extensions;
using World.Data;
using Zenject;

namespace MeshGeneration
{
    public class GPUMarchingCubesMeshGenerator : MeshGenerator, IInitializable
    {
        [Inject] private ChunkPrismsProvider _chunkSizePrismsProvider;
        [Inject] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;

        [SerializeField] private ComputeShader _meshGenerationShader;
        [SerializeField] private GenerationAlgorithmInfo _generationAlgorithmInfo;

        private ComputeBufferManager _chunkSizePrismsBufferManager;
        private ComputeBufferManager _debugDataBufferManager;
        private ComputeBufferManager _chunksDataBufferManager;
        private ComputeBufferManager _chunksPositionsBufferManager;
        private ComputeBufferManager _verticesBufferManager;
        private ComputeBufferManager _verticesInfoBufferManager;
        private NativeArrayManager<DebugData> _debugDataArrayManager;
        private NativeArrayManager<VertexInfo> _verticesInfoArrayManager;
        private NativeArrayManager<Vector3> _verticesArrayManager;

        public void Initialize()
        {
            _chunkSizePrismsBufferManager = new ComputeBufferManager(
                (count) => BuffersFactory.CreateCompute(count, typeof(RectPrismInt)));

            var chunkSizePrismsBuffer = _chunkSizePrismsBufferManager.GetObjectInstance(
                _chunkSizePrismsProvider.PrismsArray.Length);
            chunkSizePrismsBuffer.SetData(_chunkSizePrismsProvider.PrismsArray);

            _debugDataBufferManager = new ComputeBufferManager(
               (count) => BuffersFactory.CreateCompute(count, typeof(DebugData)));

            _chunksDataBufferManager = new ComputeBufferManager(
                (count) => BuffersFactory.CreateCompute(count, typeof(VoxelData)));

            _chunksPositionsBufferManager = new ComputeBufferManager(
                (count) => BuffersFactory.CreateCompute(count, typeof(Vector3Int)));

            _verticesBufferManager = new ComputeBufferManager(
                (count) => BuffersFactory.CreateCompute(count, typeof(Vector3)));

            _verticesInfoBufferManager = new ComputeBufferManager(
                (count) => BuffersFactory.CreateCompute(count, typeof(VertexInfo)));

            _debugDataArrayManager = new NativeArrayManager<DebugData>(
                (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

            _verticesInfoArrayManager = new NativeArrayManager<VertexInfo>(
                (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

            _verticesArrayManager = new NativeArrayManager<Vector3>(
                (count) => new(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));
        }

        protected void OnDestroy()
        {
            _debugDataBufferManager.Dispose();
            _chunkSizePrismsBufferManager.Dispose();
            _chunksDataBufferManager.Dispose();
            _chunksPositionsBufferManager.Dispose();
            _verticesBufferManager.Dispose();
            _verticesInfoBufferManager.Dispose();

            _debugDataArrayManager.Dispose();
            _verticesArrayManager.Dispose();
            _verticesInfoArrayManager.Dispose();
        }

        public override async Task<MeshData[]> GenerateMeshDataForChunks(
            NativeArray<Vector3Int> positions,
            List<ThreedimensionalNativeArray<VoxelData>> chunksRawData, 
            CancellationToken? cancellationToken = null)
        {
            if (positions.Length != chunksRawData.Count)
            {
                throw new ArgumentException();
            }

            RawMeshData rawMeshData = await GenerateMeshData(positions, chunksRawData, cancellationToken);

            int maxVerticesPerChunk = _chunkSizePrismsProvider.CubesPrism.Volume
                * _generationAlgorithmInfo.MaxVerticesPerMarch;

            NativeArray<GPUMeshDataFixJob> jobs = new(
                chunksRawData.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            NativeArray<JobHandle> jobHandles = new(
                chunksRawData.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            List<NativeArray<GPUMeshDataFixJobOutput>> jobsOutput = new();

            for (int i = 0; i < chunksRawData.Count; i++)
            {
                jobsOutput.Add(new(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));

                GPUMeshDataFixJob fixJob = new GPUMeshDataFixJob();

                fixJob.MaxVerticesPerChunk = maxVerticesPerChunk;
                fixJob.MaterialsCount = _materialAssociations.Count;
                fixJob.ChunkID = i;
                fixJob.InputVerticesInfo = rawMeshData.Indices;
                fixJob.InputVertices = rawMeshData.Vertices;
                fixJob.Output = jobsOutput[i];
                fixJob.ExistingMaterialHashes = _materialAssociations.GetKeysHashSet();

                fixJob.OutputVertices = new NativeArray<Vector3>(
                    maxVerticesPerChunk, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                fixJob.OutputIndices = new NativeList<int>(
                    maxVerticesPerChunk, Allocator.Persistent);

                fixJob.OutputSubmeshInfos = new NativeList<SubmeshInfo>(5, Allocator.Persistent);

                jobs[i] = fixJob;
                jobHandles[i] = fixJob.Schedule();
            }

            await AsyncUtilities.WaitWhile(() => !IsAllJobsComplete(jobHandles), 1, cancellationToken);

            foreach (var handle in jobHandles)
            {
                handle.Complete();
            }

            if (cancellationToken.IsCanceled())
            {
                return new MeshData[0];
            }

            MeshData[] result = new MeshData[chunksRawData.Count];

            for (int i = 0; i < chunksRawData.Count; i++)
            {
                GPUMeshDataFixJob job = jobs[i];
                result[i] = new MeshData(
                    job.OutputVertices, 
                    job.OutputIndices.AsArray(), 
                    job.OutputSubmeshInfos,
                    jobsOutput[i][0].IsPhysicallyCorrect,
                    jobsOutput[i][0].VerticesCount,
                    jobsOutput[i][0].VerticesCount);
            }

            jobs.Dispose();
            jobHandles.Dispose();
            
            foreach (var jobOutput in jobsOutput)
            {
                jobOutput.Dispose();
            }

            return result;
        }

        private bool IsAllJobsComplete(NativeArray<JobHandle> jobHandles)
        {
            foreach (var jobHandle in jobHandles)
            {
                if (!jobHandle.IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<RawMeshData> GenerateMeshData(
            NativeArray<Vector3Int> chunkPositions,
            List<ThreedimensionalNativeArray<VoxelData>> chunksData, 
            CancellationToken? cancellationToken = null)
        {
            int chunksCount = chunksData.Count;
            int voxelsCount = _chunkSizePrismsProvider.VoxelsPrism.Volume * chunksCount;
            int cubesCount = _chunkSizePrismsProvider.CubesPrism.Volume * chunksCount;
            int maxVerticesCount = cubesCount * _generationAlgorithmInfo.MaxVerticesPerMarch;

            ComputeBuffer chunkSizePrismsBuffer = _chunkSizePrismsBufferManager.GetObjectInstance(
                _chunkSizePrismsProvider.PrismsArray.Length);

            ComputeBuffer chunksDataBuffer = InitializeChunksDataBuffer(chunksData, voxelsCount);


            ComputeBuffer verticesInfoBuffer = _verticesInfoBufferManager.GetObjectInstance(maxVerticesCount);
            _verticesInfoArrayManager.Dispose();
            NativeArray<VertexInfo> verticesInfo = _verticesInfoArrayManager.GetObjectInstance(maxVerticesCount);
            verticesInfoBuffer.SetData(verticesInfo);

            ComputeBuffer verticesBuffer = _verticesBufferManager.GetObjectInstance(maxVerticesCount);
            _verticesArrayManager.Dispose();
            NativeArray<Vector3> vertices = _verticesArrayManager.GetObjectInstance(maxVerticesCount);
            verticesBuffer.SetData(vertices);

            int kernelId = _meshGenerationShader.FindKernel("GenerateMesh");
            _meshGenerationShader.SetBuffer(kernelId, "ChunkPrisms", chunkSizePrismsBuffer);
            _meshGenerationShader.SetBuffer(kernelId, "ChunkData", chunksDataBuffer);
            _meshGenerationShader.SetBuffer(kernelId, "Vertices", verticesBuffer);
            _meshGenerationShader.SetBuffer(kernelId, "VerticesInfo", verticesInfoBuffer);
            _meshGenerationShader.SetFloat("Surface", _generationAlgorithmInfo.SurfaceFactor);
            _meshGenerationShader.DispatchConsideringGroupSizes(
                kernelId, chunksCount, _chunkSizePrismsProvider.CubesPrism.Volume, 1);

            var verticesRequest = AsyncGPUReadback.RequestIntoNativeArray(ref vertices, verticesBuffer);
            var indicesRequest = AsyncGPUReadback.RequestIntoNativeArray(ref verticesInfo, verticesInfoBuffer);

            await AsyncUtilities.WaitWhile(() => !(verticesRequest.done && indicesRequest.done), 1, cancellationToken);
            
            if (verticesRequest.hasError)
            {
                throw new InvalidOperationException();
            }

            if (cancellationToken.IsCanceled())
            {
                return new RawMeshData();
            }

            return new RawMeshData(vertices, verticesInfo);
        }

        private ComputeBuffer InitializeChunksDataBuffer(
            List<ThreedimensionalNativeArray<VoxelData>> chunksRawData, int voxelsCount)
        {
            var chunksDataBuffer = _chunksDataBufferManager.GetObjectInstance(voxelsCount);
            int pointer = 0;
            foreach (var data in chunksRawData)
            {
                chunksDataBuffer.SetData(data.RawData, 0, pointer, data.FullLength);
                pointer += data.FullLength;
            }

            return chunksDataBuffer;
        }
    }
}
