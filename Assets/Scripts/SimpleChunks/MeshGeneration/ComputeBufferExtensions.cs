using System;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;

namespace SimpleChunks.MeshGeneration
{
    public static class ComputeBufferExtensions
    {
        public static ComputeBuffer FillBufferWithChunksData(
            this ComputeBuffer chunksDataBuffer,
            NativeArray<Vector3Int> positions,
            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData)
        {
            int pointer = 0;
            foreach (var pos in positions)
            {
                if (!chunksData.TryGetValue(PositionLongHasher.GetHashFromPosition(pos), out var data))
                {
                    throw new InvalidOperationException();
                }

                chunksDataBuffer.SetData(data.RestoreAsArray(), 0, pointer, data.Length);
                pointer += data.Length;
            }

            return chunksDataBuffer;
        }
    }
}
