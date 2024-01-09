using System.Collections.Generic;
using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using FirstSettler.Extensions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;

namespace MarchingCubesProject.Tools
{
    public struct UpdateChunkDataVoxelJob : IJobParallelFor
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

    public class MarchingCubesChunksEditor : MonoBehaviour
    {
        [SerializeField] private HeirsProvider<IChunksContainer> _chunksContainerHeir;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;

        private AffectedNeighborData[] _affectedNeighborsDataBuffer = new AffectedNeighborData[28];
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private Vector3Int _chunkSize;
        private IChunksContainer _chunksContainer;
        private bool _isAlreadyEditingChunks = false;

        protected void Awake()
        {
            _chunksContainer = _chunksContainerHeir.GetValue();
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(
                _basicChunkSettings.Size, 
                _basicChunkSettings.Scale);

            _chunkSize = _basicChunkSettings.Size;
        }

        public bool IsAlreadyEditingChunks()
        {
            return _isAlreadyEditingChunks;
        }

        public async Task SetVoxels(
            IEnumerable<ChunkPoint> newVoxels,
            Area localChunksEditingArea,
            bool updateMeshes = true)
        {
            _isAlreadyEditingChunks = true;

            Dictionary<long, IChunk> updatingChunks = new Dictionary<long, IChunk>();

            foreach (var chunkDataVoxel in newVoxels)
            {
                Vector3Int localChunkPosition = chunkDataVoxel.LocalChunkPosition.FloorToVector3Int();
                Vector3Int localChunkDataPoint = chunkDataVoxel.LocalChunkDataPoint.FloorToVector3Int();

                FillAffectedNeighborsDataBuffer(localChunkPosition, localChunkDataPoint, out int elementsCount);

                for (int i = 0; i < elementsCount; i++)
                {
                    localChunkPosition = _affectedNeighborsDataBuffer[i].AffectedLocalChunkPosition;
                    localChunkDataPoint = _affectedNeighborsDataBuffer[i].AffectedLocalChunkDataPoint;

                    IChunk affectedChunk = _chunksContainer.GetChunk(
                    localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

                    if (affectedChunk == null)
                    {
                        continue;
                    }

                    var voxelData = new VoxelData()
                    {
                        Volume = chunkDataVoxel.Volume,
                        MaterialHash = chunkDataVoxel.MaterialHash
                    };

                    affectedChunk.ChunkData.SetVoxelData(
                        localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z, voxelData
                        );

                    if (!updatingChunks.ContainsKey(affectedChunk.GetUniqueIndex()))
                    {
                        updatingChunks.Add(affectedChunk.GetUniqueIndex(), affectedChunk);
                    }
                }
            }

            try
            {
                if (updateMeshes)
                {
                    await UpdateMeshes(updatingChunks);
                }
            }
            finally
            {
                _isAlreadyEditingChunks = false;
            }
        }

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
                var voxelData = chunk.ChunkData.GetVoxelData(
                    localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);
                return new ChunkPoint(localChunkPosition, localChunkDataPoint, voxelData.Volume, voxelData.MaterialHash);
            }
            else
            {
                return new ChunkPoint();
            }
           
        }

        public ChunkPoint GetChunkDataPoint(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            IChunk chunk = _chunksContainer.GetChunk(
                localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

            if (chunk == null)
            {
                return default;
            }

            var voxelData = chunk.ChunkData.GetVoxelData(
                    localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);

            return new ChunkPoint(localChunkPosition, localChunkDataPoint, voxelData.Volume, voxelData.MaterialHash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task UpdateMeshes(Dictionary<long, IChunk> updatingChunks)
        {
            foreach (var updatingChunk in updatingChunks)
            {
                await updatingChunk.Value.GenerateNewMeshData();
            }

            foreach (var updatingChunk in updatingChunks)
            {
                updatingChunk.Value.ApplyMeshData();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillAffectedNeighborsDataBuffer(
            Vector3Int localChunkPosition, Vector3Int localChunkDataPoint, out int elementsCount)
        {
            int count = 0;
            Vector3Int newChunkDataPoint = localChunkDataPoint;
            Vector3Int newLocalChunkPosition = localChunkPosition;

            Vector3Int affectMask = Vector3Int.zero;

            for (int i = 0; i < 3; i++)
            {
                if (localChunkDataPoint[i] == _chunkSize[i])
                    affectMask[i] = 1;
                else if (localChunkDataPoint[i] == 0)
                    affectMask[i] = -1;

                if (affectMask[i] != 0)
                {
                    newChunkDataPoint[i] = affectMask[i] == -1 ? _chunkSize[i] : 0;
                    newLocalChunkPosition[i] = localChunkPosition[i] + affectMask[i];
                }
            }

            for (int number = 0; number <= 7; number++)
            {
                Vector3Int tmpChunkDataPoint = localChunkDataPoint;
                Vector3Int tmpLocalChunkPosition = localChunkPosition;

                for (int i = 0; i < 3; i++)
                {
                    if ((number & (1 << i)) == 0 && affectMask[i] != 0)
                    {
                        tmpChunkDataPoint[i] = newChunkDataPoint[i];
                        tmpLocalChunkPosition[i] = newLocalChunkPosition[i];
                    }
                }

                if (!IsChunkDataInBuffer(count, tmpChunkDataPoint))
                {
                    _affectedNeighborsDataBuffer[count] = new AffectedNeighborData(
                        affectMask, tmpLocalChunkPosition, tmpChunkDataPoint);

                    count++;
                }
            }

            elementsCount = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAllChunksDataApplied(Dictionary<long, IChunk> updatingChunks)
        {
            bool isAllChunksDataApplied = true;

            foreach (var item in updatingChunks)
            {
                if (!item.Value.IsMeshDataApplying())
                {
                    isAllChunksDataApplied = false;
                    break;
                }
            }

            return isAllChunksDataApplied;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsChunkDataInBuffer(int elementsInBuffer, Vector3Int chunkDataPoint)
        {
            bool isInBuffer = false;
            for (int i = 0; i < elementsInBuffer; i++)
            {
                if (_affectedNeighborsDataBuffer[i].AffectedLocalChunkDataPoint == chunkDataPoint)
                {
                    isInBuffer = true;
                    break;
                }
            }

            return isInBuffer;
        }
    }
}
