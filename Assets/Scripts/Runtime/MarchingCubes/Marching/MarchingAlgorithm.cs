using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject
{
    public abstract class MarchingAlgorithm : IMeshGenerationAlgorithm
    {
        private GenerationAlgorithmInfo _generationAlgorithmInfo ;
        private int[] _windingOrder = new int[] { 0, 1, 2 };
        private float[] _cube = new float[8];

        public float Surface { get; set; }
        public GenerationAlgorithmInfo MeshGenerationAlgorithmInfo { get => _generationAlgorithmInfo; }
        protected int[] WindingOrder { get => _windingOrder; }

        protected MarchingAlgorithm(float surface, GenerationAlgorithmInfo generationAlgorithmInfo)
        {
            Surface = surface;
            _generationAlgorithmInfo = generationAlgorithmInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenerateMeshData(ChunkData chunkData, MeshDataBuffer cashedMeshData)
        {
            MeshDataBuffer localMeshData = cashedMeshData;
            cashedMeshData.ResetAllCollections();
            UpdateWindingOrder();
            int width = chunkData.Width;
            int height = chunkData.Height;
            int depth = chunkData.Depth;

            int widthMinusOne = width - 1;
            int heightMinusOne = height - 1;
            int depthMinusOne = chunkData.Depth - 1;

            int mainChunkPartElementsCount = widthMinusOne * heightMinusOne * depthMinusOne;
            int fullChunkElementsCount = width * height * depth;

            GenerateMainPartOfChunkMeshData(
                chunkData, localMeshData, widthMinusOne, heightMinusOne, mainChunkPartElementsCount);
        }


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
        }

        protected abstract MeshDataBuffer March(
            float x, float y, float z, float[] cube, MeshDataBuffer meshData, int materialHash);

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
        private void GenerateMainPartOfChunkMeshData(
            ChunkData chunkData,
            MeshDataBuffer localMeshData,
            int width,
            int height,
            int total)
        {
            for (int i = 0; i < total; i++)
            {
                int x = i % width;
                int y = (i / width) % height;
                int z = i / (width * height);

                ApplyVertexOffsets(chunkData, x, y, z, _cube);
                int voxelHash = chunkData.GetVoxelData(x, y, z).MaterialHash;
                localMeshData = March(x, y, z, _cube, localMeshData, voxelHash);
            }
        }

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
