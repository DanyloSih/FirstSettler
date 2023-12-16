using System.Runtime.CompilerServices;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public abstract class MarchingAlgorithm
    {
        public abstract int MaxTrianglesPerMarch { get; }
        public abstract int MaxVerticesPerMarch { get; }
        public float Surface { get; set; }
        protected int[] WindingOrder { get; private set; }

        private float[] _cube = new float[8];

        protected MarchingAlgorithm(float surface)
        {
            Surface = surface;
            WindingOrder = new int[] { 0, 1, 2 };
        }

        public virtual MeshData GenerateMeshData(ChunkData chunkData, MeshData cashedMeshData, ChunkNeighbors neighbors)
        {
            MeshData localMeshData = cashedMeshData;
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


            return localMeshData;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateWindingOrder()
        {
            if (Surface > 0.0f)
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }
            else
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
        }

        protected abstract MeshData March(
            float x, float y, float z, float[] cube, MeshData meshData, int materialHash);

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
            MeshData localMeshData,
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
                var voxelHash = chunkData.GetMaterialHash(x, y, z);
                localMeshData = March(x, y, z, _cube, localMeshData, voxelHash);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVertexOffsets(ChunkData chunkData, int x, int y, int z, float[] cube)
        {
            cube[0] = chunkData.GetVolume(x, y, z);
            cube[1] = chunkData.GetVolume(x + 1, y, z);
            cube[2] = chunkData.GetVolume(x + 1, y + 1, z);
            cube[3] = chunkData.GetVolume(x, y + 1, z);
            cube[4] = chunkData.GetVolume(x, y, z + 1);
            cube[5] = chunkData.GetVolume(x + 1, y, z + 1);
            cube[6] = chunkData.GetVolume(x + 1, y + 1, z + 1);
            cube[7] = chunkData.GetVolume(x, y + 1, z + 1);
        }
    }
}
