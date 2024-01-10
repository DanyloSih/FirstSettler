using UnityEngine;
using World.Data;
using FirstSettler.Extensions;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Utilities.Math;
using Unity.Collections;
using System;

namespace MarchingCubesProject.Tools
{
    public struct UpdateChunkDataVoxelJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int ChunkSize;
        [ReadOnly] public Parallelepiped ChunkDataModel;
        [ReadOnly] public NativeArray<ChunkPoint> NewVoxels;
        [ReadOnly] public NativeHashMap<long, IntPtr> AffectedChunksDataPointers;

        public void Execute(int index)
        {
            ChunkPoint chunkVoxel = NewVoxels[index];
            if (!chunkVoxel.IsInitialized)
            {
                return;
            }

            Vector3Int localChunkPosition = chunkVoxel.LocalChunkPosition.FloorToVector3Int();
            Vector3Int localChunkDataPoint = chunkVoxel.LocalChunkDataPoint.FloorToVector3Int();

            NativeList<AffectedNeighborData> affectedNeighborsDataBuffer = GetAffectedNeighborsData(
                localChunkPosition, localChunkDataPoint, out int elementsCount);

            for (int i = 0; i < elementsCount; i++)
            {
                localChunkPosition = affectedNeighborsDataBuffer[i].AffectedLocalChunkPosition;
                localChunkDataPoint = affectedNeighborsDataBuffer[i].AffectedLocalChunkDataPoint;

                long positionHash = PositionHasher.GetPositionHash(
                    localChunkPosition.x, localChunkPosition.y, localChunkPosition.z);

                if (!AffectedChunksDataPointers.ContainsKey(positionHash))
                {
                    continue;
                }

                IntPtr rawDataStartPointer = AffectedChunksDataPointers[positionHash];
   
                var voxelData = new VoxelData()
                {
                    Volume = chunkVoxel.Volume,
                    MaterialHash = chunkVoxel.MaterialHash
                };

                int voxelDataOffset = ChunkDataModel.VoxelPositionToIndex(
                    localChunkDataPoint.x, localChunkDataPoint.y, localChunkDataPoint.z);

                unsafe
                {
                    VoxelData* dataPointer = (VoxelData*)rawDataStartPointer.ToPointer();
                    dataPointer[voxelDataOffset] = voxelData;
                }
            }

            affectedNeighborsDataBuffer.Dispose();
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
                    affectedNeighborsData.Add(new AffectedNeighborData(
                        affectMask, tmpLocalChunkPosition, tmpChunkDataPoint));

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
