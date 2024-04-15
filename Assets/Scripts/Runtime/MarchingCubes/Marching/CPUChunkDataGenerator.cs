using System.Threading.Tasks;
using ProceduralNoiseProject;
using UnityEngine;
using Utilities.Math;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public class CPUChunkDataGenerator : MonoBehaviour, IChunkDataProvider
    {
        [SerializeField] private float _maxHeight = 256;
        [SerializeField] private float _minHeight;
        [SerializeField] private int _octaves;
        [SerializeField] private float _frequency;
        [SerializeField] private int _seed = 0;
        [SerializeField] private Vector3Int _chunksOffset;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private FractalNoise _fractal;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        public Task FillChunkData(ChunkData chunkData, int chunkLocalX, int chunkLocalY, int chunkLocalZ)
        {
            ThreedimensionalNativeArray<VoxelData> voxels = chunkData.VoxelsData;
            int width = (voxels.Width - 1);
            int height = (voxels.Height - 1);
            int depth = (voxels.Depth - 1);
            int chunkGlobalX = chunkLocalX * width;
            int chunkGlobalY = chunkLocalY * height;
            int chunkGlobalZ = chunkLocalZ * depth;

            for (int y = 0; y <= height; y++)
            {
                float voxelGlobalY = chunkGlobalY + y;
                for (int x = 0; x <= width; x++)
                {
                    float voxelGlobalX = chunkGlobalX + x;
                    for (int z = 0; z <= depth; z++)
                    {
                        float voxelGlobalZ = chunkGlobalZ + z;
                        VoxelData result = new VoxelData();

                        result.Volume = voxelGlobalY <= _minHeight ? 1 : 0;
                        result.MaterialHash = _heightAssociations.GetMaterialKeyHashByHeight(voxelGlobalY);

                        voxels.SetValue(x, y, z, result);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
