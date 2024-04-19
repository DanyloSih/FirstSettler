using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace MarchingCubesProject
{
    public struct CPUChunkDataGenerationJob : IJobParallelFor
    {
        public Vector3Int ChunkGlobalPosition;
        public ChunkGenerationSettings ChunkGenerationSettings;

        private Vector3Int _chunkSize;
        [WriteOnly]
        private NativeArray<VoxelData> _voxels;
        [ReadOnly]
        private NativeHeightAndMaterialHashAssociations _associations;
        private RectPrismInt _voxelsPrism;

        public CPUChunkDataGenerationJob(
            NativeArray<VoxelData> voxels,
            RectPrismInt voxelsPrism,
            Vector3Int chunkGlobalPosition,
            ChunkGenerationSettings chunkGenerationSettings,
            NativeHeightAndMaterialHashAssociations associations)
        {
            _voxels = voxels;
            ChunkGlobalPosition = chunkGlobalPosition;
            _voxelsPrism = voxelsPrism;
            _chunkSize = voxelsPrism.Size - Vector3Int.one;
            ChunkGenerationSettings = chunkGenerationSettings;
            _associations = associations;
        }

        public void Execute(int index)
        {
            Vector3Int localVoxelPos = _voxelsPrism.IndexToPoint(index);
            Vector3Int globalVoxelPos = ChunkGlobalPosition + localVoxelPos;
            VoxelData result = new VoxelData();

            result.Volume = globalVoxelPos.y <= ChunkGenerationSettings.MinHeight ? 1 : 0;
            result.MaterialHash = _associations.GetMaterialHashByHeight(globalVoxelPos.y);

            _voxels[index] = result;
        }
    }
}
