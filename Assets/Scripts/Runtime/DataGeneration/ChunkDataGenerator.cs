using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;
using World.Data;
using World.Organization;

namespace DataGeneration
{
    public abstract class ChunkDataGenerator : MonoBehaviour, IChunkDataProvider
    {
        public abstract Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions,
            CancellationToken? cancellationToken = null);
    }
}
