using System.Collections.Generic;
using SimpleHeirs;
using UnityEngine;
using World.Data;
using World.Organization;
using FirstSettler.Extensions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>> affectedChunksData 
                = GetAffectedChunksVoxelData(localChunksEditingArea, _basicChunkSettings.Size);

            foreach (var chunkDataVoxel in newVoxels)
            {
                Vector3Int localChunkPosition = chunkDataVoxel.LocalChunkPosition.FloorToVector3Int();
                Vector3Int localChunkDataPoint = chunkDataVoxel.LocalChunkDataPoint.FloorToVector3Int();

                var affectedNeighborsDataBuffer = GetAffectedNeighborsData(
                    localChunkPosition, localChunkDataPoint, out int elementsCount);

                for (int i = 0; i < elementsCount; i++)
                {
                    localChunkPosition = affectedNeighborsDataBuffer[i].AffectedLocalChunkPosition;
                    localChunkDataPoint = affectedNeighborsDataBuffer[i].AffectedLocalChunkDataPoint;

                    long positionHash = PositionHasher.GetPositionHash(
                        localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

                    if (!affectedChunksData.ContainsKey(positionHash))
                    {
                        continue;
                    }

                    var voxelData = new VoxelData()
                    {
                        Volume = chunkDataVoxel.Volume,
                        MaterialHash = chunkDataVoxel.MaterialHash
                    };

                    affectedChunksData[positionHash].SetValue(
                        localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z, voxelData
                        );
                }
            }

            foreach (var affectedChunkData in affectedChunksData)
            {
                IChunk affectedChunk = _chunksContainer.GetChunk(affectedChunkData.Key);
                affectedChunk.ChunkData.VoxelsData.SetNewRawData(ref affectedChunkData.Value.RawData);
            }     

            try
            {
                if (updateMeshes)
                {
                    await UpdateMeshes(affectedChunksData);
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

        private NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>> GetAffectedChunksVoxelData(
            Area affectedArea, Vector3Int chunksSize)
        {
            Vector3Int affectedAreaSize = affectedArea.Parallelepiped.Size;
            int maxXChunks = affectedAreaSize.x / chunksSize.x + 1;
            int maxYChunks = affectedAreaSize.y / chunksSize.y + 1;
            int maxZChunks = affectedAreaSize.z / chunksSize.z + 1;
            int maxAffectedChunksCount = maxXChunks * maxYChunks * maxZChunks;

            NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>> result 
                = new NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>>(maxAffectedChunksCount, Allocator.Persistent);

            for (int y = Mathf.FloorToInt((float)affectedArea.Min.y / chunksSize.y) * chunksSize.y; y < affectedArea.Max.y; y += chunksSize.y)
            {
                for (int x = Mathf.FloorToInt((float)affectedArea.Min.x / chunksSize.x) * chunksSize.x; x < affectedArea.Max.x; x += chunksSize.x)
                {
                    for (int z = Mathf.FloorToInt((float)affectedArea.Min.z / chunksSize.z) * chunksSize.z; z < affectedArea.Max.z; z += chunksSize.z)
                    {
                        int localChunkX = x / chunksSize.x;
                        int localChunkY = y / chunksSize.y;
                        int localChunkZ = z / chunksSize.z;
                        IChunk chunk = _chunksContainer.GetChunk(localChunkX, localChunkY, localChunkZ);

                        if (chunk == null)
                        {
                            continue;
                        }

                        result.Add(
                            PositionHasher.GetPositionHash(localChunkX, localChunkY, localChunkZ), 
                            chunk.ChunkData.VoxelsData);
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task UpdateMeshes(object packedThreedimensionalNativeArray)
        {
            NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>> updatingChunks 
                = (NativeParallelHashMap<long, ThreedimensionalNativeArray<VoxelData>>)packedThreedimensionalNativeArray;

            foreach (var updatingChunk in updatingChunks)
            {
                var chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                await chunk.GenerateNewMeshData();
            }

            foreach (var updatingChunk in updatingChunks)
            {
                var chunk = _chunksContainer.GetChunk(updatingChunk.Key);
                chunk.ApplyMeshData();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArray<AffectedNeighborData> GetAffectedNeighborsData(
            Vector3Int localChunkPosition, Vector3Int localChunkDataPoint, out int elementsCount)
        {
            var affectedNeighborsData = new NativeArray<AffectedNeighborData>(28, Allocator.Temp);

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

                if (!IsChunkDataInBuffer(count, tmpChunkDataPoint, ref affectedNeighborsData))
                {
                    affectedNeighborsData[count] = new AffectedNeighborData(
                        affectMask, tmpLocalChunkPosition, tmpChunkDataPoint);

                    count++;
                }
            }

            elementsCount = count;
            return affectedNeighborsData;
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
        private bool IsChunkDataInBuffer(int elementsInBuffer, Vector3Int chunkDataPoint, ref NativeArray<AffectedNeighborData> affectedNeighborsData)
        {
            bool isInBuffer = false;
            for (int i = 0; i < elementsInBuffer; i++)
            {
                if (affectedNeighborsData[i].AffectedLocalChunkDataPoint == chunkDataPoint)
                {
                    isInBuffer = true;
                    break;
                }
            }

            return isInBuffer;
        }
    }
}
