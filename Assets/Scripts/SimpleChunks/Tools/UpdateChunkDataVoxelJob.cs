using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using Utilities.Jobs;
using SimpleChunks.DataGeneration;

namespace SimpleChunks.Tools
{
    public struct UpdateChunkDataVoxelJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int ChunkSize;
        [ReadOnly] public RectPrismInt ChunkDataModel;
        [ReadOnly] public NativeArray<ChunkPointWithData> NewVoxels;
        [ReadOnly] public NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly AffectedChunksDataPointers;

        public void Execute(int index)
        {
            ChunkPointWithData chunkVoxel = NewVoxels[index];
            if (!chunkVoxel.IsInitialized)
            {
                return;
            }
            
            Vector3Int localChunkPosition = Vector3Int.FloorToInt(chunkVoxel.LocalChunkPosition);
            Vector3Int localChunkDataPoint = Vector3Int.FloorToInt(chunkVoxel.LocalVoxelPosition);

            if (ChunkDataModel.IsSurfacePoint(localChunkDataPoint))
            {
                NativeList<AffectedNeighborData> affectedNeighborsDataBuffer = GetAffectedNeighborsData(
                    localChunkPosition, localChunkDataPoint, out int elementsCount);

                for (int i = 0; i < elementsCount; i++)
                {
                    localChunkPosition = affectedNeighborsDataBuffer[i].AffectedLocalChunkPosition;
                    localChunkDataPoint = affectedNeighborsDataBuffer[i].AffectedLocalChunkDataPoint;
                    ApplyVoxelData(localChunkPosition, localChunkDataPoint, chunkVoxel.VoxelData);
                }
                affectedNeighborsDataBuffer.Dispose();
            }
            else
            {
                ApplyVoxelData(localChunkPosition, localChunkDataPoint, chunkVoxel.VoxelData);
            }   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVoxelData(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint, VoxelData voxelData)
        {
            int positionHash = PositionIntHasher.GetHashFromPosition(localChunkPosition);

            if (AffectedChunksDataPointers.ContainsKey(positionHash))
            {
                int voxelDataOffset = ChunkDataModel.PointToIndex(localChunkDataPoint);
                var chunkData = AffectedChunksDataPointers[positionHash];
                chunkData[voxelDataOffset] = voxelData;
            }  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeList<AffectedNeighborData> GetAffectedNeighborsData(
            Vector3Int localChunkPosition, Vector3Int localChunkDataPoint, out int elementsCount)
        {
            var affectedNeighborsData = new NativeList<AffectedNeighborData>(2, Allocator.TempJob);

            int count = 0;
            Vector3Int newChunkDataPoint = localChunkDataPoint;
            Vector3Int newLocalChunkPosition = localChunkPosition;

            Vector3Int affectMask = Vector3Int.zero;

            for (int i = 0; i < 3; i++)
            {
                if (localChunkDataPoint[i] == ChunkSize[i])
                    affectMask[i] = 1;
                else if (localChunkDataPoint[i] == 0)
                    affectMask[i] = -1;

                if (affectMask[i] != 0)
                {
                    newChunkDataPoint[i] = affectMask[i] == -1 ? ChunkSize[i] : 0;
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

                if (!IsChunkDataInBuffer(count, tmpChunkDataPoint, affectedNeighborsData))
                {
                    affectedNeighborsData.Add(new AffectedNeighborData(tmpLocalChunkPosition, tmpChunkDataPoint));

                    count++;
                }
            }

            elementsCount = count;
            return affectedNeighborsData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsChunkDataInBuffer(int elementsInBuffer, Vector3Int chunkDataPoint, NativeList<AffectedNeighborData> affectedNeighborsData)
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
