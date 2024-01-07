﻿using System.Collections.Generic;
using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using FirstSettler.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesChunksEditor : MonoBehaviour
    {
        [SerializeField] private HeirsProvider<IChunksContainer> _chunksContainerHeir;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;

        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private Vector3Int _chunkScaledSize;
        private Vector3Int _chunkSize;
        private IChunksContainer _chunksContainer;

        private void Awake()
        {
            _chunksContainer = _chunksContainerHeir.GetValue();
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(
                _basicChunkSettings.Size, 
                _basicChunkSettings.Scale);

            _chunkScaledSize = _basicChunkSettings.ScaledSize;
            _chunkSize = _basicChunkSettings.Size;
        }

        public async void SetNewChunkDataVolumeAndMaterial(
            IEnumerable<ChunkDataPoint> chunkDataVolumeAndMaterials, 
            bool updateMeshes = true)
        {
            Dictionary<long, IChunk> updatingChunks = new Dictionary<long, IChunk>();
            foreach (var chunkDataVoxel in chunkDataVolumeAndMaterials)
            {
                Vector3Int localChunkPosition = chunkDataVoxel.LocalChunkPosition.FloorToVector3Int();
                Vector3Int localChunkDataPoint = chunkDataVoxel.LocalChunkDataPoint.FloorToVector3Int();

                List<AffectedNeighborData> affectedNeighborsData
                    = GetAffectedNeighborsData(
                        localChunkPosition,
                        localChunkDataPoint);

                foreach (var affectedNeighborData in affectedNeighborsData)
                {
                    localChunkPosition = affectedNeighborData.AffectedLocalChunkPosition;
                    localChunkDataPoint = affectedNeighborData.AffectedLocalChunkDataPoint;

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

            if (updateMeshes)
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
        }

        public ChunkDataPoint GetChunkDataPoint(Vector3 globalChunkDataPoint)
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
                return new ChunkDataPoint(globalChunkDataPoint, localChunkPosition, localChunkDataPoint, voxelData.Volume, voxelData.MaterialHash);
            }
            else
            {
                return new ChunkDataPoint();
            }
           
        }

        public ChunkDataPoint GetChunkDataPoint(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            Vector3 globalChunkPos = _chunkCoordinatesCalculator
                .GetGlobalChunkDataPointByLocalChunkAndPoint(localChunkPosition, localChunkDataPoint);

            IChunk chunk = _chunksContainer.GetChunk(
                localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

            var voxelData = chunk.ChunkData.GetVoxelData(
                    localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);

            return new ChunkDataPoint(globalChunkPos, localChunkPosition, localChunkDataPoint, voxelData.Volume, voxelData.MaterialHash);
        }

        private List<AffectedNeighborData> GetAffectedNeighborsData(
            Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            List<AffectedNeighborData> result = new List<AffectedNeighborData>();
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

                if (!result.Any(x => x.AffectedLocalChunkDataPoint == tmpChunkDataPoint))
                {
                    result.Add(new AffectedNeighborData(affectMask, tmpLocalChunkPosition, tmpChunkDataPoint));
                }
            }

            return result;
        }
    }
}
