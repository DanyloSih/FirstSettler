using UnityEngine;
using Unity.Collections;

namespace World.Data
{
    public class MeshDataBuffer
    {
        public int VerticesCount;
        public NativeArray<Vector3> VerticesCash;
        public NativeArray<TriangleAndMaterialHash> TrianglesCash;
        public NativeArray<Vector2> UVsCash;

        public MeshDataBuffer(int maxVerticesCount)
        {
            VerticesCount = 0;
            VerticesCash = new NativeArray<Vector3>(maxVerticesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            TrianglesCash = new NativeArray<TriangleAndMaterialHash>(maxVerticesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            UVsCash = new NativeArray<Vector2>(maxVerticesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        public void DisposeAllArrays()
        {      
            if (VerticesCash != default && !VerticesCash.Equals(null))
            {
                VerticesCash.Dispose();
            }

            if (TrianglesCash != default && !TrianglesCash.Equals(null))
            {
                TrianglesCash.Dispose();
            }

            if (UVsCash != default && !UVsCash.Equals(null))
            {
                UVsCash.Dispose();
            }

            VerticesCount = 0;
        }
    }
}
