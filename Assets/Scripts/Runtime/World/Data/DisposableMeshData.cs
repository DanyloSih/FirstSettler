using UnityEngine;
using Unity.Collections;

namespace World.Data
{
    public class DisposableMeshData : IMeshDataHandler
    {
        public NativeArray<Vector3> VerticesCash = default;
        public NativeArray<TriangleAndMaterialHash> TrianglesCash = default;
        public NativeArray<Vector2> UVsCash = default;

        private int _arraysUpdatesNumber = 0;

        public void UpdateTriangles(NativeArray<TriangleAndMaterialHash> triangles)
        {
            ReadBackCallBack(triangles, ref TrianglesCash);
        }

        public void UpdateUVs(NativeArray<Vector2> uvs)
        {
            ReadBackCallBack(uvs, ref UVsCash);
        }

        public void UpdateVertices(NativeArray<Vector3> vertices)
        {
            ReadBackCallBack(vertices, ref VerticesCash);
        }

        public void DisposeAllArrays()
        {
            _arraysUpdatesNumber = 0;

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
        }

        public bool IsAllArraysUpdated()
        {
            return _arraysUpdatesNumber == 3;
        }

        private void ReadBackCallBack<T>(NativeArray<T> data, ref NativeArray<T> cash)
            where T : struct
        {
            cash = new NativeArray<T>(data.Length, Allocator.Persistent);
            data.CopyTo(cash);
            _arraysUpdatesNumber++;
        }
    }
}
