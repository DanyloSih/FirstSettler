using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Threading;
using static UnityEngine.Mesh;

namespace World.Data
{
    public class MeshDataBuffersReader : IDisposable
    {
        private static MeshBuffers s_meshBuffers;

        private int _maxVerticesCount;
        private int[] _polygonsCount = new int[1];
        private int _currentVertices = 0;

        public int PolygonsCount { get => _polygonsCount[0]; private set => _polygonsCount[0] = value; }   
        public int VerticesCount { get => _currentVertices; private set => _currentVertices = value; }

        public MeshDataBuffersReader(int maxVerticesCount)
        {
            _maxVerticesCount = maxVerticesCount;
            if (s_meshBuffers == null)
            {
                s_meshBuffers = new MeshBuffers(_maxVerticesCount);
            }
        }

        public MeshBuffers CreateNewMeshBuffers()
        {
            return s_meshBuffers;
        }

        public void UpdatePolygonsCount(MeshBuffers meshBuffers)
        {
            var argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(meshBuffers.PolygonsCounter, argBuffer, 0);
            argBuffer.GetData(_polygonsCount);
            argBuffer.Dispose();
            VerticesCount = PolygonsCount * 3;
        }

        public async System.Threading.Tasks.Task<MeshData> ReadFromBuffersToMeshData(MeshBuffers meshBuffers)
        {
            MeshData meshData = null;

            if (PolygonsCount != 0)
            {
                meshData = new MeshData(VerticesCount);
                AsyncGPUReadbackRequest verticesRequest = AsyncGPUReadback.RequestIntoNativeArray(
                    ref meshData.VerticesCash, meshBuffers.VerticesBuffer, sizeof(float) * 3 * VerticesCount, 0);

                AsyncGPUReadbackRequest trianglesRequest = AsyncGPUReadback.RequestIntoNativeArray(
                    ref meshData.TrianglesCash, meshBuffers.TrianglesBuffer, sizeof(int) * 2 * VerticesCount, 0);

                AsyncGPUReadbackRequest uvsRequest = AsyncGPUReadback.RequestIntoNativeArray(
                    ref meshData.UVsCash, meshBuffers.UVBuffer, sizeof(float) * 2 * VerticesCount, 0);

                await AsyncUtilities.WaitWhile(() => !verticesRequest.done || !trianglesRequest.done || !uvsRequest.done, 5);
            }
            else
            {
                meshData = new MeshData(0);
            }

            return meshData;
        }

        public void Dispose()
        {
            s_meshBuffers.DisposeAllBuffers();
        }
    }
}
