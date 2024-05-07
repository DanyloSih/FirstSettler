using UnityEngine;
using Unity.Collections;

namespace World.Data
{
    public struct RawMeshData
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<IndexAndMaterialHash> Indices;

        public RawMeshData(NativeArray<Vector3> vertices, NativeArray<IndexAndMaterialHash> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
