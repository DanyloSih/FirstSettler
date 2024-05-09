using UnityEngine;
using Unity.Collections;
using System;

namespace MeshGeneration
{
    public struct MeshData : IDisposable
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> SortedIndices;
        public NativeList<SubmeshInfo> SubmeshesInfo;
        public bool IsPhysicallyCorrect;
        public int VerticesCount;
        public int IndicesCount;

        public MeshData(
            NativeArray<Vector3> vertices,
            NativeArray<int> sortedIndices,
            NativeList<SubmeshInfo> submeshesInfo,
            bool isPhysicallyCorrect,
            int verticesCount,
            int indicesCount)
        {
            Vertices = vertices;
            SortedIndices = sortedIndices;
            SubmeshesInfo = submeshesInfo;
            IsPhysicallyCorrect = isPhysicallyCorrect;
            VerticesCount = verticesCount;
            IndicesCount = indicesCount;
        }

        public void Dispose()
        {
            Vertices.Dispose();
            SortedIndices.Dispose();
            SubmeshesInfo.Dispose();
        }
    }
}
