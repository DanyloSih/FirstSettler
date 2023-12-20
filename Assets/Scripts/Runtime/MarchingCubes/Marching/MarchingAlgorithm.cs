using System;
using System.Runtime.CompilerServices;
using FirstSettler.Extensions;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject
{
    public abstract class MarchingAlgorithm : IMeshGenerationAlgorithm
    {
        private GenerationAlgorithmInfo _generationAlgorithmInfo;
        private ComputeBuffer _windingOrderBuffer;
        private int[] _windingOrder = new int[] { 0, 1, 2 };

        public float Surface { get; set; }
        public GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get => _generationAlgorithmInfo; }
        public ComputeBuffer WindingOrderBuffer { get => _windingOrderBuffer; }

        protected MarchingAlgorithm(GenerationAlgorithmInfo generationAlgorithmInfo, float surface)
        {
            Surface = surface;
            _generationAlgorithmInfo = generationAlgorithmInfo;
            _windingOrderBuffer = ComputeBufferExtensions.Create(3, typeof(int));
            _windingOrderBuffer.SetData(_windingOrder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void GenerateMeshData(ChunkData chunkData, MeshDataBuffers cashedMeshData);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateWindingOrder()
        {
            if (Surface > 0.0f)
            {
                _windingOrder[0] = 2;
                _windingOrder[1] = 1;
                _windingOrder[2] = 0;
            }
            else
            {
                _windingOrder[0] = 0;
                _windingOrder[1] = 1;
                _windingOrder[2] = 2;
            }
            _windingOrderBuffer.SetData(_windingOrder);
        }

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
