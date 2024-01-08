using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace World.Data
{
    public class MeshDataBuffersReader
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

        public async System.Threading.Tasks.Task<DisposableMeshData> GetAllDataFromBuffers(MeshBuffers meshBuffers)
        {
            DisposableMeshData meshDataHandler = new DisposableMeshData();

            if (PolygonsCount != 0)
            {
                AsyncGPUReadbackRequest verticesRequest = AsyncGPUReadback.Request(
                    meshBuffers.VerticesBuffer, sizeof(float) * 3 * VerticesCount, 0, 
                    x => { meshDataHandler.UpdateVertices(x.GetData<Vector3>()); });
 
                AsyncGPUReadbackRequest trianglesRequest = AsyncGPUReadback.Request(
                    meshBuffers.TrianglesBuffer, sizeof(int) * 2 * VerticesCount, 0,
                    x => { meshDataHandler.UpdateTriangles(x.GetData<TriangleAndMaterialHash>()); });

                AsyncGPUReadbackRequest uvsRequest = AsyncGPUReadback.Request(
                    meshBuffers.UVBuffer, sizeof(float) * 2 * VerticesCount, 0,
                    x => { meshDataHandler.UpdateUVs(x.GetData<Vector2>()); });

                await UniTask.WaitWhile(() => !meshDataHandler.IsAllArraysUpdated(), PlayerLoopTiming.PreUpdate);
            }

            meshBuffers.DisposeAllBuffers();
            return meshDataHandler;
        }
    }
}
