using UnityEngine;
using Unity.Collections;

namespace SimpleChunks.MeshGeneration
{
    public struct RawMeshData
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<VertexInfo> Indices;

        public RawMeshData(NativeArray<Vector3> vertices, NativeArray<VertexInfo> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
