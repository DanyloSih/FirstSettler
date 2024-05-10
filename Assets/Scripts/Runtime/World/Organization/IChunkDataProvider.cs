using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace World.Organization
{
    public interface IChunkDataProvider
    {
        public Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions, 
            CancellationToken? cancellationToken = null);
    }
}
