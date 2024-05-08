using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ProceduralNoiseProject;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using World.Data;
using Zenject;

namespace MarchingCubesProject
{
    public class CPUChunkDataGenerator : MonoBehaviour
    {
        [Inject] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;

        [SerializeField] private ChunkGenerationSettings _chunkGenerationSettings;
        [SerializeField] private MaterialKeyAndHeightAssociations _heightAssociations;

        private FractalNoise _fractal;
        private NativeHeightAndMaterialHashAssociations _nativeHeightAndMaterialHashAssociations;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        protected void OnDisable()
        {
            _nativeHeightAndMaterialHashAssociations.Dispose();
        }

        public async Task GenerateChunksRawData(ThreedimensionalNativeArray<VoxelData> voxels, Vector3Int chunkGlobalPosition)
        {
            _nativeHeightAndMaterialHashAssociations = _heightAssociations
                .GetOrCreateNative(Allocator.Persistent);

            var generationJob = new CPUChunkDataGenerationJob(
                voxels.RawData,
                voxels.RectPrism,
                chunkGlobalPosition,
                _chunkGenerationSettings,
                _nativeHeightAndMaterialHashAssociations);

            var jobHandler = generationJob.Schedule(voxels.RawData.Length, 128);
            await jobHandler.WaitAsync(PlayerLoopTiming.EarlyUpdate);
        }
    }
}
