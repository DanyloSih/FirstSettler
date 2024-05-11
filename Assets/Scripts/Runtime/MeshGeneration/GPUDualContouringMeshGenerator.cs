using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Shaders;
using World.Data;
using Zenject;

namespace MeshGeneration
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
            List<ThreedimensionalNativeArray<VoxelData>> chunksData,
            CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
