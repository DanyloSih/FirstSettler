using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks.MeshGeneration
{
    public struct MeshGenerationArgs
    {
        public NativeArray<Vector3Int> Positions;
        public NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>.ReadOnly ChunksData;
        public CancellationToken? CancellationToken;
        public TaskCompletionSource<MeshData[]> TaskCompletionSource;

        public MeshGenerationArgs(
            NativeArray<Vector3Int> positions,
            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData,
            CancellationToken? cancellationToken,
            TaskCompletionSource<MeshData[]> taskCompletionSource)
        {
            ChunksData = chunksData;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
            Positions = positions;
        }
    }
}
