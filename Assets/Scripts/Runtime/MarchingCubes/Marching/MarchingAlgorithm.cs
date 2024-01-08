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

    }
}
