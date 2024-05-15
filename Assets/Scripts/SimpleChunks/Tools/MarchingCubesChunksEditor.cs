using UnityEngine;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using Zenject;
using Utilities.Threading;
using Utilities.Jobs;
using System.Threading;
using SimpleChunks.MeshGeneration;
using SimpleChunks.DataGeneration;
using System;

namespace SimpleChunks.Tools
{
    public class MarchingCubesChunksEditor
    {
        [Inject] private BasicChunkSettings _basicChunkSettings;
        [Inject] private ChunksContainer _chunksContainer;
        [Inject] private ChunkPrismsProvider _prismsProvider;
        [Inject] private MeshGenerator _meshGenerator;
        [Inject] private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;

        private Vector3Int _chunkSize;
        private bool _isAlreadyEditingChunks = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlreadyEditingChunks()
        {
            return _isAlreadyEditingChunks;
        }

        public async Task UpdateMeshes(
            NativeArray<Vector3Int> affectedChunksPositions,
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly affectedChunks,
            CancellationToken? cancellationToken = null)
        {
            MeshData[] meshes = await _meshGenerator.GenerateMeshDataForChunks(
                affectedChunksPositions, affectedChunks, cancellationToken);

            int counter = 0;
            foreach (var chunkPosition in affectedChunksPositions)
            {
                int chunkHash = PositionIntHasher.GetHashFromPosition(chunkPosition);
                if (_chunksContainer.TryGetValue(chunkHash, out var chunkObject))
                {
                    chunkObject.ApplyMeshData(meshes[counter]);
                    meshes[counter].Dispose();
                }
                else
                {
                    throw new ArgumentException($"{nameof(ChunksContainer)} does not " +
                        $"contain chunk with position hash: {chunkHash}");
                }

                counter++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SetVoxels(
            NativeArray<ChunkPointWithData> newVoxels,
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
                    await UpdateMeshes(
                        ChunksDataToPositions(chunksDataPointersInsideEditArea), 
                        chunksDataPointersInsideEditArea, 
                        cancellationToken);
                }
            }
            finally
            {
                _isAlreadyEditingChunks = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkPointWithData GetChunkDataPoint(Vector3 globalChunkDataPoint)
        {
            Vector3Int localChunkPosition = _chunkCoordinatesCalculator
                .GetLocalChunkPositionByGlobalPoint(globalChunkDataPoint);

            Vector3Int localChunkDataPoint = _chunkCoordinatesCalculator
                .GetLocalChunkDataPointByGlobalPoint(globalChunkDataPoint);

            _chunksContainer.TryGetValue(
                localChunkPosition.x, localChunkPosition.y, localChunkPosition.z, out var chunk);

            if (chunk != null)
            {
                VoxelData voxelData = chunk.ChunkData.GetValue(localChunkDataPoint);
                return new ChunkPointWithData(localChunkPosition, localChunkDataPoint, voxelData);
            }
            else
            {
                return new ChunkPointWithData();
            }
        }

        private NativeArray<Vector3Int> ChunksDataToPositions(
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData)
        {
            int chunksCount = chunksData.Count();
            NativeArray<Vector3Int> positions = new (chunksCount, Allocator.Persistent);
            int counter = 0;
            foreach (var chunkData in chunksData)
            {
                positions[counter] = PositionIntHasher.GetPositionFromHash(chunkData.Key);
                counter++;
            }

            return positions;
        }
    }
}
