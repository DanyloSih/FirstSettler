using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FirstSettler.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityThreading;

using Task = System.Threading.Tasks.Task;
using System;

namespace World.Data
{
    public class MeshDataBuffersKeeper
    {
        public Vector3[] CashedVertices;
        public TriangleAndMaterialHash[] CashedTriangles;
        public Vector2[] CashedUV;
        private int _maxVerticesCount;
        private MonoBehaviour _coroutineExecutor;
        private int[] _polygonsCount = new int[1];
        private int _currentVertices = 0;

        private Dictionary<int, List<int>> _materialKeyAndTriangleListAssociations 
            = new Dictionary<int, List<int>>();
        private MeshBuffers _meshBuffers;
        private Coroutine _coroutine;

        public int PolygonsCount { get => _polygonsCount[0]; private set => _polygonsCount[0] = value; }   
        public int VerticesCount { get => _currentVertices; private set => _currentVertices = value; }   

        public MeshDataBuffersKeeper(int maxVerticesCount, MonoBehaviour coroutineExecutor)
        {
            CashedVertices = new Vector3[maxVerticesCount];
            CashedTriangles = new TriangleAndMaterialHash[maxVerticesCount];
            CashedUV = new Vector2[maxVerticesCount];
            _maxVerticesCount = maxVerticesCount;
            _coroutineExecutor = coroutineExecutor;
        }

        public MeshBuffers GetOrCreateNewMeshBuffers()
        {
            if (_meshBuffers == null)
            {
                _meshBuffers = new MeshBuffers(
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(Vector3), ComputeBufferType.Counter, ComputeBufferMode.Immutable),
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(TriangleAndMaterialHash), ComputeBufferType.Counter, ComputeBufferMode.Immutable),
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(Vector2), ComputeBufferType.Counter, ComputeBufferMode.Immutable),
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(int), ComputeBufferType.Counter));
            }

            return _meshBuffers;  
        }

        public void UpdatePolygonsCount()
        {
            var meshBuffers = GetOrCreateNewMeshBuffers();

            var argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(meshBuffers.PolygonsCounter, argBuffer, 0);
            argBuffer.GetData(_polygonsCount);
            VerticesCount = PolygonsCount * 3;
        }

        public async Task GetAllDataFromBuffers(MeshBuffers meshBuffers = null)
        {
            if (_coroutine != null)
            {
                throw new InvalidOperationException($"Method {nameof(GetAllDataFromBuffers)} already executing! " +
                    $"Wait for ending before invoke.");
            }

            if (meshBuffers == null)
            {
                meshBuffers = GetOrCreateNewMeshBuffers();
            }

            if (PolygonsCount != 0)
            {
                AsyncGPUReadbackRequest verticesRequest = AsyncGPUReadback.Request(meshBuffers.VerticesBuffer);
                AsyncGPUReadbackRequest trianglesRequest = AsyncGPUReadback.Request(meshBuffers.TrianglesBuffer);
                AsyncGPUReadbackRequest uvsRequest = AsyncGPUReadback.Request(meshBuffers.UvsBuffer);
                
                _coroutine = _coroutineExecutor.StartCoroutine(
                    ReceivingDataProcess(verticesRequest, trianglesRequest, uvsRequest));
            }

            await UniTask.WaitWhile(() => _coroutine != null);
            //meshBuffers.VerticesBuffer.GetData(CashedVertices, 0, 0, VerticesCount);
            //meshBuffers.TrianglesBuffer.GetData(CashedTriangles, 0, 0, VerticesCount);
            //meshBuffers.UvsBuffer.GetData(CashedUV, 0, 0, VerticesCount); 
        }

        public void DisposeBuffers()
        {
            var meshBuffers = GetOrCreateNewMeshBuffers();

            meshBuffers.VerticesBuffer?.Dispose();
            meshBuffers.TrianglesBuffer?.Dispose();
            meshBuffers.UvsBuffer?.Dispose();
            meshBuffers.PolygonsCounter?.Dispose();

            _meshBuffers = null;
        }

        public void UpdateMeshEssentialsFromCash()
        {
            _materialKeyAndTriangleListAssociations.Clear();
            for (int j = 0; j < VerticesCount; j++)
            {
                TriangleAndMaterialHash newInfo = CashedTriangles[j];
                if (_materialKeyAndTriangleListAssociations.ContainsKey(newInfo.MaterialHash))
                {
                    _materialKeyAndTriangleListAssociations[newInfo.MaterialHash].Add(newInfo.Triangle);
                }
                else
                {
                    _materialKeyAndTriangleListAssociations.Add(newInfo.MaterialHash, new List<int>() { newInfo.Triangle });
                }
            }
        }

        public IEnumerable<KeyValuePair<int, List<int>>> GetMaterialKeyHashAndTriangleListAssociations()
            => _materialKeyAndTriangleListAssociations;

        public void ResetAllCollections()
        {
            PolygonsCount = 0;
            VerticesCount = 0;
            _materialKeyAndTriangleListAssociations.Clear();
        }

        private IEnumerator ReceivingDataProcess(
            AsyncGPUReadbackRequest verticesRequest,
            AsyncGPUReadbackRequest trianglesRequest,
            AsyncGPUReadbackRequest uvsRequest)
        {
            while (!verticesRequest.done || !trianglesRequest.done || !uvsRequest.done) 
            {
                yield return null;
            }

            verticesRequest.GetData<Vector3>().CopyTo(CashedVertices);
            trianglesRequest.GetData<TriangleAndMaterialHash>().CopyTo(CashedTriangles);
            uvsRequest.GetData<Vector2>().CopyTo(CashedUV);
            _coroutine = null;
        }
    }
}
