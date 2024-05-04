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
        public Task<NativeList<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions, 
            Vector3Int chunkOffset, 
            Vector3Int chunkDataSize,
            CancellationToken? cancellationToken = null);

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
