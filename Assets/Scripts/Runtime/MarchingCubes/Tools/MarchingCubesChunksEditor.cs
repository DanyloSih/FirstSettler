using System.Collections.Generic;
using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using FirstSettler.Extensions;
using System.Linq;

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

        public void SetNewChunkDataVolumeAndMaterial(
            IEnumerable<ChunkDataPoint> chunkDataVolumeAndMaterials, 
            bool updateMeshes = true)
        {
            List<IChunk> updatingChunks = new List<IChunk>();

            foreach (var chunkDataVoxel in chunkDataVolumeAndMaterials)
            {
                Vector3Int localChunkPosition 
                    = _chunkCoordinatesCalculator.GetLocalChunkPositionByGlobalPoint(
                        chunkDataVoxel.GlobalChunkDataPoint);

                Vector3Int localChunkDataPoint
                    = _chunkCoordinatesCalculator.GetLocalChunkDataPointByGlobalPoint(
                        chunkDataVoxel.GlobalChunkDataPoint);

                List<AffectedNeighborData> affectedNeighborsData
                    = GetAffectedNeighborsData(localChunkPosition, localChunkDataPoint);

                foreach (var affectedNeighborData in affectedNeighborsData)
                {
                    Vector3Int chunkPosition = affectedNeighborData.AffectedLocalChunkPosition;
                    IChunk affectedChunk = _chunksContainer.GetChunk(
                        chunkPosition.x, chunkPosition.y, chunkPosition.z);

                    if (affectedChunk == null)
                    {
                        continue;
                    }

                    Vector3Int dataPoint = affectedNeighborData.AffectedLocalChunkDataPoint;
                    affectedChunk.ChunkData.SetVolume(
                        dataPoint.x, dataPoint.y, dataPoint.z, chunkDataVoxel.Volume);
                    affectedChunk.ChunkData.SetMaterialHash(
                        dataPoint.x, dataPoint.y, dataPoint.z, chunkDataVoxel.MaterialHash);

                    if (!updatingChunks.Contains(affectedChunk))
                    {
                        updatingChunks.Add(affectedChunk);
                    }
                }
            }

            if (updateMeshes)
            {
                foreach (var updatingChunk in updatingChunks)
                {
                    updatingChunk.UpdateMesh();
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
                float volume = chunk.ChunkData.GetVolume(
               localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);
                int materialHash = chunk.ChunkData.GetMaterialHash(
                    localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);

                return new ChunkDataPoint(globalChunkDataPoint, volume, materialHash);
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

            float volume = chunk.ChunkData.GetVolume(
                localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);
            int materialHash = chunk.ChunkData.GetMaterialHash(
                localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);

            return new ChunkDataPoint(globalChunkPos, volume, materialHash);
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
