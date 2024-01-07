using UnityEngine;
using Unity.Collections;

namespace World.Data
{
    public interface IMeshDataHandler
    {
        public void UpdateVertices(NativeArray<Vector3> vertices);
        public void UpdateTriangles(NativeArray<TriangleAndMaterialHash> triangles);
        public void UpdateUVs(NativeArray<Vector2> uvs);
    }
}
