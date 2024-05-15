using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Shaders;
using Zenject;

namespace SimpleChunks.MeshGeneration
{
    public class GPUDualContouringMeshGenerator : MeshGenerator
    {
        [Inject] private ChunkPrismsProvider _chunkSizePrismsProvider;
        [Inject] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;

        [SerializeField] private ComputeShader _meshGenerationShader;

        private ComputeBufferManager _chunkSizePrismsBufferManager;
        private ComputeBufferManager _chunksDataBufferManager;
        private ComputeBufferManager _verticesBufferManager;
        private ComputeBufferManager _verticesInfoBufferManager;
        private NativeArrayManager<VertexInfo> _verticesInfoArrayManager;
        private NativeArrayManager<Vector3> _verticesArrayManager;

        protected override Task<MeshData[]> OnGenerateMeshDataForChunks(
            NativeArray<Vector3Int> positions,
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData,
            CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
