using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FirstSettler.Extensions;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject
{
    public abstract class MarchingAlgorithm : IMeshGenerationAlgorithm
    {
        private GenerationAlgorithmInfo _generationAlgorithmInfo;
        private ComputeBuffer _windingOrderBuffer;

        public float Surface { get; set; }
        public GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get => _generationAlgorithmInfo; }

        protected MarchingAlgorithm(GenerationAlgorithmInfo generationAlgorithmInfo, float surface)
        {
            Surface = surface;
            _generationAlgorithmInfo = generationAlgorithmInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task<DisposableMeshData> GenerateMeshData(ChunkData chunkData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        protected static readonly int[,] VertexOffset = new int[,]
        {
            {0, 0, 0}, {1, 0, 0}, {1, 1, 0}, {0, 1, 0},
            {0, 0, 1}, {1, 0, 1}, {1, 1, 1}, {0, 1, 1}
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVertexOffsets(ChunkData chunkData, int x, int y, int z, float[] cube)
        {
            cube[0] = chunkData.GetVoxelData(x, y, z).Volume;
            cube[1] = chunkData.GetVoxelData(x + 1, y, z).Volume;
            cube[2] = chunkData.GetVoxelData(x + 1, y + 1, z).Volume;
            cube[3] = chunkData.GetVoxelData(x, y + 1, z).Volume;
            cube[4] = chunkData.GetVoxelData(x, y, z + 1).Volume;
            cube[5] = chunkData.GetVoxelData(x + 1, y, z + 1).Volume;
            cube[6] = chunkData.GetVoxelData(x + 1, y + 1, z + 1).Volume;
            cube[7] = chunkData.GetVoxelData(x, y + 1, z + 1).Volume;
        }
    }
}
