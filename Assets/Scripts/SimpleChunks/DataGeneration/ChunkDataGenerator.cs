using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks.DataGeneration
{
    public abstract class ChunkDataGenerator : MonoBehaviour, IChunkDataProvider
    {
        public abstract Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions,
            CancellationToken? cancellationToken = null);
    }
}
