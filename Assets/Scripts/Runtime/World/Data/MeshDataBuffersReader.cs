using System;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Threading;

namespace World.Data
{
    public class MeshDataBuffersReader : IDisposable
    {
        private int _maxVerticesCount;
        private int[] _polygonsCount = new int[1];
        private int _currentVertices = 0;

        public int PolygonsCount { get => _polygonsCount[0]; private set => _polygonsCount[0] = value; }   
        public int VerticesCount { get => _currentVertices; private set => _currentVertices = value; }

        public MeshDataBuffersReader(int maxVerticesCount)
        {
            _maxVerticesCount = maxVerticesCount;
        }

        public MeshBuffers CreateNewMeshBuffers()
        {
            return new MeshBuffers(_maxVerticesCount);
        }

        public void UpdatePolygonsCount(MeshBuffers meshBuffers)
        {
            var argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(meshBuffers.PolygonsCounter, argBuffer, 0);
            argBuffer.GetData(_polygonsCount);
            argBuffer.Dispose();
            VerticesCount = PolygonsCount * 3;
        }

        public async System.Threading.Tasks.Task<MeshDataBuffer> ReadFromBuffersToMeshData(MeshBuffers meshBuffers)
        {
            MeshDataBuffer meshData = null;
            if (PolygonsCount != 0)
            {
                meshData = new MeshDataBuffer(VerticesCount);
                meshData.VerticesCount = VerticesCount;
                meshData.VerticesCount = VerticesCount;
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
                meshData = new MeshDataBuffer(0);
                meshData.VerticesCount = VerticesCount;
            }

            return meshData;
        }

        public void Dispose()
        {

        }
    }
}
