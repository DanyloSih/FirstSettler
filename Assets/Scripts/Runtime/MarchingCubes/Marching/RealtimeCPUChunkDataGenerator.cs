using System.Threading.Tasks;
using ProceduralNoiseProject;
using UnityEngine;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public class RealtimeCPUChunkDataGenerator : MonoBehaviour, IChunkDataProvider
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

        public async Task<ChunkData> GetChunkData(int x, int y, int z, Vector3Int chunkSize)
        {
            Vector3Int chunkDataSize = chunkSize + new Vector3Int(1, 1, 1) * 1;
            var voxels = new MultidimensionalArray<VoxelData>(chunkDataSize);

            await FillVoxelsArray(voxels, x, y, z);
            return new ChunkData(voxels);
        }

        private Task FillVoxelsArray(MultidimensionalArray<VoxelData> voxels, int x, int y, int z)
        {
            int mat = _heightAssociations.GetMaterialKeyHashByHeight(0);
            int chunkGlobalXPos = x * (voxels.Width - 1);
            int chunkGlobalYPos = y * (voxels.Height - 1);
            int chunkGlobalZPos = z * (voxels.Depth - 1);



            return Task.CompletedTask;
        }
    }
}
