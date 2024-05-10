using System.Collections.Generic;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace MeshGeneration.Extensions
{
    public static class ComputeBufferExtensions
    {
        public static ComputeBuffer FillBufferWithChunksData(
            this ComputeBuffer chunksDataBuffer,
            List<ThreedimensionalNativeArray<VoxelData>> chunksRawData)
        {
            int pointer = 0;
            foreach (var data in chunksRawData)
            {
                chunksDataBuffer.SetData(data.RawData, 0, pointer, data.FullLength);
                pointer += data.FullLength;
            }

            return chunksDataBuffer;
        }
    }
}
