using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using Utilities.Math;

namespace SimpleChunks.MeshGeneration
{
    public struct MeshGenerationArgs
    {
        public List<ThreedimensionalNativeArray<VoxelData>> ChunksData;
        public CancellationToken? CancellationToken;
        public TaskCompletionSource<MeshData[]> TaskCompletionSource;

        public MeshGenerationArgs(
            List<ThreedimensionalNativeArray<VoxelData>> chunksData,
            CancellationToken? cancellationToken,
            TaskCompletionSource<MeshData[]> taskCompletionSource)
        {
            ChunksData = chunksData;
            CancellationToken = cancellationToken;
            TaskCompletionSource = taskCompletionSource;
        }
    }
}
