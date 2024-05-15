using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;

namespace SimpleChunks.DataGeneration
{
    public abstract class ChunkDataGenerator : MonoBehaviour, IChunkDataProvider
    {
        public abstract Task<NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>> GenerateChunksRawData(
            NativeArray<Vector3Int> generatingChunksLocalPositions,
            CancellationToken? cancellationToken = null);
    }
}
