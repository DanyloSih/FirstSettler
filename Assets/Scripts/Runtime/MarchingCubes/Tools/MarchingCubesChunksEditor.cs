using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Zenject;
using Utilities.Threading;
using System.Collections.Generic;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesChunksEditor : MonoBehaviour
    {
        [SerializeField] private HeirsProvider<IChunksContainer> _chunksContainerHeir;

        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private Vector3Int _chunkSize;
        private IChunksContainer _chunksContainer;
        private bool _isAlreadyEditingChunks = false;
        private BasicChunkSettings _basicChunkSettings;

        [Inject]
        public void Construct(BasicChunkSettings basicChunkSettings)
        {
            _basicChunkSettings = basicChunkSettings;
        }

        protected void Awake()
        {
            _chunksContainer = _chunksContainerHeir.GetValue();
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
            setVoxelsJob.ChunkDataModel = new Parallelepiped(_chunkSize + Vector3Int.one);

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
                newVoxels.Dispose();
                chunksDataPointersInsideEditArea.Dispose();
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
        public ChunkPoint GetChunkDataPoint(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            IChunk chunk = _chunksContainer.GetChunk(
                localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

            if (chunk == null)
            {
                return default;
            }

            var voxelData = chunk.ChunkData.GetVoxelData(localChunkDataPoint);
            return new ChunkPoint(localChunkPosition, localChunkDataPoint, voxelData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeHashMap<long, IntPtr> GetAffectedChunksDataPointers(
            Area affectedArea, Vector3Int chunksSize, IChunksContainer chunksContainer)
        {
            Vector3Int affectedAreaSize = affectedArea.Parallelepiped.Size;
            int maxXChunks = affectedAreaSize.x / chunksSize.x + affectedAreaSize.x % chunksSize.x == 0 ? 0 : 1;
            int maxYChunks = affectedAreaSize.y / chunksSize.y + affectedAreaSize.y % chunksSize.y == 0 ? 0 : 1;
            int maxZChunks = affectedAreaSize.z / chunksSize.z + affectedAreaSize.z % chunksSize.z == 0 ? 0 : 1;
            int maxAffectedChunksCount = maxXChunks * maxYChunks * maxZChunks;

            NativeHashMap<long, IntPtr> pointers
                = new NativeHashMap<long, IntPtr>(maxAffectedChunksCount, Allocator.Persistent);

            for (int y = Mathf.FloorToInt((float)affectedArea.Min.y / chunksSize.y) * chunksSize.y; y < affectedArea.Max.y; y += chunksSize.y)
            {
                for (int x = Mathf.FloorToInt((float)affectedArea.Min.x / chunksSize.x) * chunksSize.x; x < affectedArea.Max.x; x += chunksSize.x)
                {
                    for (int z = Mathf.FloorToInt((float)affectedArea.Min.z / chunksSize.z) * chunksSize.z; z < affectedArea.Max.z; z += chunksSize.z)
                    {
                        int localChunkX = x / chunksSize.x;
                        int localChunkY = y / chunksSize.y;
                        int localChunkZ = z / chunksSize.z;
                        int positionHash = PositionHasher.GetPositionHash(localChunkX, localChunkY, localChunkZ);
                        IChunk chunk = chunksContainer.GetChunk(positionHash);

                        if (chunk == null)
                        {
                            continue;
                        }

                        unsafe
                        {
                            pointers.Add(positionHash, new IntPtr(chunk.ChunkData.VoxelsData.RawData.GetUnsafePtr()));
                        };
                    }
                }
            }

            return pointers;
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
