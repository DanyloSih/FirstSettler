using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FirstSettler.Extensions;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject
{
    public class MarchingCubesAlgorithm : MarchingAlgorithm
    {
        private ComputeShader _meshGenerationComputeShader;
        private Vector3 _vertex1 = default;
        private Vector3 _vertex2 = default;
        private Vector3 _vertex3 = default;
        private Vector2 _uv1 = default;
        private Vector2 _uv2 = default;
        private Vector2 _uv3 = default;
        private Vector3 _normal;
        private Quaternion _rotation;

        public MarchingCubesAlgorithm(
			GenerationAlgorithmInfo generationAlgorithmInfo, 
			ComputeShader meshGenerationComputeShader,
            float surface)
            : base(generationAlgorithmInfo, surface)
        {
            _meshGenerationComputeShader = meshGenerationComputeShader;
        }

        public override void GenerateMeshData(ChunkData chunkData, MeshDataBuffersKeeper meshBuffersKeeper)
        {
            meshBuffersKeeper.ResetAllCollections();
            MultidimensionalArray<VoxelData> voxels = chunkData.VoxelsData;
            ComputeBuffer voxelsBuffer = voxels.GetOrCreateVoxelsDataBuffer();
            MeshBuffers meshBuffers = meshBuffersKeeper.GetOrCreateNewMeshBuffers();

            int kernelId = _meshGenerationComputeShader.FindKernel("CSMain");
            _meshGenerationComputeShader.SetBuffer(kernelId, "ChunkData", voxelsBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "Triangles", meshBuffers.TrianglesBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "Vertices", meshBuffers.VerticesBuffer);
            _meshGenerationComputeShader.SetBuffer(kernelId, "UVs", meshBuffers.UvsBuffer);
            _meshGenerationComputeShader.SetInt("MaxVericesCount", MeshGenerationAlgorithmInfo.MaxVerticesPerMarch);
            _meshGenerationComputeShader.SetInt("ChunkWidth", voxels.Width);
            _meshGenerationComputeShader.SetInt("ChunkHeight", voxels.Height);
            _meshGenerationComputeShader.SetInt("ChunkDepth", voxels.Depth);
            _meshGenerationComputeShader.SetFloat("Surface", Surface);
            _meshGenerationComputeShader.Dispatch(
                kernelId, voxels.Width - 1, voxels.Height - 1, voxels.Depth - 1);

            voxels.GetDataFromVoxelsBuffer(voxelsBuffer);
            meshBuffersKeeper.GetAllDataFromBuffers(meshBuffers);

            List<Vector2> debug = new List<Vector2>();
            foreach (var item in meshBuffersKeeper.CashedUV)
            {
                if(item != Vector2.zero)
                {
                    debug.Add(item);
                }
            }
        }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    protected override MeshDataBuffer March(float x, float y, float z, float[] cube, MeshDataBuffer cashedMeshData, int materialHash)
        //    {
        //        int i, j, vert, idx;
        //        int flagIndex = 0;
        //        float offset = 0.0f;
        //        List<int> triangles = cashedMeshData.GetTrianglesListByMaterialKeyHash(materialHash);

        //        for (i = 0; i < 8; i++) 
        //if (cube[i] <= Surface) 
        //	flagIndex |= 1 << i;

        //        int edgeFlags = CubeEdgeFlags[flagIndex];

        //        if (edgeFlags == 0) 
        //return cashedMeshData;

        //        for (i = 0; i < 12; i++)
        //        {
        //            if ((edgeFlags & (1 << i)) != 0)
        //            {
        //                offset = GetOffset(cube[EdgeConnection[i, 0]], cube[EdgeConnection[i, 1]]);
        //                _edgeVertex[i].x = x + VertexOffset[EdgeConnection[i, 0], 0] + offset * EdgeDirection[i, 0];
        //                _edgeVertex[i].y = y + VertexOffset[EdgeConnection[i, 0], 1] + offset * EdgeDirection[i, 1];
        //                _edgeVertex[i].z = z + VertexOffset[EdgeConnection[i, 0], 2] + offset * EdgeDirection[i, 2];
        //}
        //        }

        //        for (i = 0; i < 5; i++)
        //        {
        //            if (TriangleConnectionTable[flagIndex, 3 * i] < 0) break;

        //            idx = cashedMeshData.VerticesTargetLength;

        //            for (j = 0; j < 3; j++)
        //            {
        //                vert = TriangleConnectionTable[flagIndex, 3 * i + j];
        //                triangles.Add(idx + WindingOrder[j]);
        //                cashedMeshData.CashedVertices[cashedMeshData.VerticesTargetLength] = _edgeVertex[vert];
        //                cashedMeshData.VerticesTargetLength++;
        //            }

        //            UpdateUV(cashedMeshData);
        //        }

        //        return cashedMeshData;
        //    }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateUV(MeshDataBuffersKeeper cashedMeshData)
        {
            _vertex1 = cashedMeshData.CashedVertices[cashedMeshData.VerticesTargetLength - 3];
            _vertex2 = cashedMeshData.CashedVertices[cashedMeshData.VerticesTargetLength - 2];
            _vertex3 = cashedMeshData.CashedVertices[cashedMeshData.VerticesTargetLength - 1];

			UpdateTriangleUVProjection();

            cashedMeshData.UvTargetLength += 3;
            cashedMeshData.CashedUV[cashedMeshData.UvTargetLength - 3] = _uv1;
            cashedMeshData.CashedUV[cashedMeshData.UvTargetLength - 2] = _uv2;
            cashedMeshData.CashedUV[cashedMeshData.UvTargetLength - 1] = _uv3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateTriangleUVProjection()
		{												
            _normal = Vector3.Cross(_vertex2 - _vertex1, _vertex3 - _vertex1).normalized;
            _rotation = Quaternion.FromToRotation(_normal, Vector3.up);
            _vertex1 = _rotation * _vertex1;
            _vertex2 = _rotation * _vertex2;
            _vertex3 = _rotation * _vertex3;

			_uv1.x = _vertex1.x;
			_uv1.y = _vertex1.z;
			_uv2.x = _vertex2.x;
			_uv2.y = _vertex2.z;
			_uv3.x = _vertex3.x;
			_uv3.y = _vertex3.z;
        }
    }

}
