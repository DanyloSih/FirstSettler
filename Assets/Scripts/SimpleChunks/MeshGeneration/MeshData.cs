using UnityEngine;
using Unity.Collections;
using System;

namespace SimpleChunks.MeshGeneration
{
    public struct MeshData : IDisposable
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> SortedIndices;
        public NativeList<SubmeshInfo> SubmeshesInfo;
        public bool IsPhysicallyCorrect;
        public int VerticesCount;
        public int IndicesCount;

        private NativeList<int> _sortedIndicesList;

        public MeshData(
            NativeArray<Vector3> vertices,
            NativeList<int> sortedIndices,
            NativeList<SubmeshInfo> submeshesInfo,
            bool isPhysicallyCorrect,
            int verticesCount,
            int indicesCount)
        {
            Vertices = vertices;
            _sortedIndicesList = sortedIndices;
            SortedIndices = sortedIndices.AsArray();
            SubmeshesInfo = submeshesInfo;
            IsPhysicallyCorrect = isPhysicallyCorrect;
            VerticesCount = verticesCount;
            IndicesCount = indicesCount;
        }

        public void Dispose()
        {
            Vertices.Dispose();
            SortedIndices.Dispose();
            _sortedIndicesList.Dispose();
            SubmeshesInfo.Dispose();
        }
    }
}
