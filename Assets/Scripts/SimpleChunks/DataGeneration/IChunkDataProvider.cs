using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks.DataGeneration
{
    public interface IChunkDataProvider
    {
        public Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions, 
            CancellationToken? cancellationToken = null);
    }
}
