using UnityEngine;
using World.Data;
using World.Organization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using Zenject;
using Utilities.Threading;
using System.Collections.Generic;
using MeshGeneration;
using Utilities.Jobs;
using System.Threading;
using Utilities.Threading.Extensions;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesChunksEditor
    {
        [Inject] private ChunkPrismsProvider _prismsProvider;
        [Inject] private MeshGenerator _meshGenerator;

        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private Vector3Int _chunkSize;
        private IChunksContainer _chunksContainer;
        private bool _isAlreadyEditingChunks = false;
        private BasicChunkSettings _basicChunkSettings;

        [Inject]
        public MarchingCubesChunksEditor(BasicChunkSettings basicChunkSettings, IChunksContainer chunksContainer)
        {
            _basicChunkSettings = basicChunkSettings;
            _chunksContainer = chunksContainer;

            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(
                _basicChunkSettings.Size,
                _basicChunkSettings.Scale);

            _chunkSize = _basicChunkSettings.Size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlreadyEditingChunks()
        {
            return _isAlreadyEditingChunks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SetVoxels(
            NativeArray<ChunkPoint> newVoxels,
            int voxelsCount,
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly chunksDataPointersInsideEditArea,
            bool updateMeshes = true,
            CancellationToken? cancellationToken = null)
        {
            _isAlreadyEditingChunks = true;

            UpdateChunkDataVoxelJob setVoxelsJob = new UpdateChunkDataVoxelJob();
            setVoxelsJob.NewVoxels = newVoxels;
            setVoxelsJob.AffectedChunksDataPointers = chunksDataPointersInsideEditArea;
            setVoxelsJob.ChunkSize = _chunkSize;
            setVoxelsJob.ChunkDataModel = new RectPrismInt(_chunkSize + Vector3Int.one);

            JobHandle jobHandle = setVoxelsJob.Schedule(voxelsCount, 8);
            await AsyncUtilities.WaitWhile(() => !jobHandle.IsCompleted, 1, cancellationToken);
            jobHandle.Complete();

            try
            {
                if (updateMeshes)
                {
                    await UpdateMeshes(chunksDataPointersInsideEditArea, cancellationToken);
                }
            }
            finally
            {
                _isAlreadyEditingChunks = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkPoint GetChunkDataPoint(Vector3 globalChunkDataPoint)
        {
            Vector3Int localChunkPosition = _chunkCoordinatesCalculator
                .GetLocalChunkPositionByGlobalPoint(globalChunkDataPoint);

            Vector3Int localChunkDataPoint = _chunkCoordinatesCalculator
                .GetLocalChunkDataPointByGlobalPoint(globalChunkDataPoint);

            IChunk chunk = _chunksContainer.GetChunk(
                localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

            if (chunk != null)
            {
                VoxelData voxelData = chunk.ChunkData.GetValue(localChunkDataPoint);
                return new ChunkPoint(localChunkPosition, localChunkDataPoint, voxelData);
            }
            else
            {
                return new ChunkPoint();
            }
           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task UpdateMeshes(
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly affectedChunksDataPointers,
            CancellationToken? cancellationToken = null)
        {
            int length = affectedChunksDataPointers.Count();

            List<ThreedimensionalNativeArray<VoxelData>> chunksData = new();
            NativeArray<Vector3Int> positions = new(length, Allocator.Persistent);
            int counter = 0;

            unsafe
            {
                foreach (var updatingChunk in affectedChunksDataPointers)
                {
                    IChunk chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                    int dataLength = _prismsProvider.VoxelsPrism.Volume;
                    positions[counter] = chunk.LocalPosition;
   
                    chunksData.Add(new ThreedimensionalNativeArray<VoxelData>(
                        updatingChunk.Value.RestoreAsArray(), _prismsProvider.VoxelsPrism.Size));

                    counter++;
                }
            }

            var chunksMeshData = await _meshGenerator.GenerateMeshDataForChunks(chunksData, cancellationToken)
                .OnException(ex => Debug.LogException(ex));

            positions.Dispose();
            foreach (var item in chunksData)
            {
                item.Dispose();
            }

            counter = 0;
            foreach (var updatingChunk in affectedChunksDataPointers)
            {
                var chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                chunk.ApplyMeshData(chunksMeshData[counter]);
                chunksMeshData[counter].Dispose();
                counter++;
            }
        }
    }
}
