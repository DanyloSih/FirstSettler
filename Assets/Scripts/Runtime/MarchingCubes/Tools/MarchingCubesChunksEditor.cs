using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using System;
using Zenject;
using Utilities.Threading;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesChunksEditor
    {
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
            NativeHashMap<int, IntPtr> chunksDataPointersInsideEditArea,
            bool updateMeshes = true)
        {
            _isAlreadyEditingChunks = true;

            UpdateChunkDataVoxelJob setVoxelsJob = new UpdateChunkDataVoxelJob();
            setVoxelsJob.NewVoxels = newVoxels;
            setVoxelsJob.AffectedChunksDataPointers = chunksDataPointersInsideEditArea;
            setVoxelsJob.ChunkSize = _chunkSize;
            setVoxelsJob.ChunkDataModel = new RectPrismInt(_chunkSize + Vector3Int.one);

            JobHandle jobHandle = setVoxelsJob.Schedule(voxelsCount, 8);
            await AsyncUtilities.WaitWhile(() => !jobHandle.IsCompleted);
            jobHandle.Complete();

            try
            {
                if (updateMeshes)
                {
                    await UpdateMeshes(chunksDataPointersInsideEditArea);
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
                VoxelData voxelData = chunk.ChunkData.GetVoxelData(localChunkDataPoint);
                return new ChunkPoint(localChunkPosition, localChunkDataPoint, voxelData);
            }
            else
            {
                return new ChunkPoint();
            }
           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task UpdateMeshes(NativeHashMap<int, IntPtr> affectedChunksDataPointers)
        {
            int length = affectedChunksDataPointers.Count;
            Task[] generationTasks = new Task[length];

            int counter = 0;
            foreach (var updatingChunk in affectedChunksDataPointers)
            {
                var chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                generationTasks[counter] = chunk.GenerateNewMeshData();
                counter++;
            }

            await Task.WhenAll(generationTasks);

            foreach (var updatingChunk in affectedChunksDataPointers)
            {
                var chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                chunk.ApplyMeshData();
            }
        }
    }
}
