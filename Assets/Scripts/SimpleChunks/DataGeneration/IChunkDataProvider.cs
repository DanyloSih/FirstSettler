using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;

namespace SimpleChunks.DataGeneration
{
    public interface IChunkDataProvider
    {
        public Task<NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions, 
            CancellationToken? cancellationToken = null);
    }
}
