using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Collections;
using UnityEngine.SceneManagement;

namespace World.Data
{
    public class MeshDataBuffersReader
    {
        private static Dictionary<int, MeshBuffers> s_meshBuffers;

        private int _maxVerticesCount;
        private MonoBehaviour _coroutineExecutor;
        private int[] _polygonsCount = new int[1];
        private int _currentVertices = 0;
        private int _coroutinesCount = 0;
        private bool _isTest = false;

        public int PolygonsCount { get => _polygonsCount[0]; private set => _polygonsCount[0] = value; }   
        public int VerticesCount { get => _currentVertices; private set => _currentVertices = value; }

        static MeshDataBuffersReader()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (s_meshBuffers == null)
            {
                s_meshBuffers = new Dictionary<int, MeshBuffers>();
            }
            else
            {
                DisposeBuffers();
            }
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            DisposeBuffers();
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            DisposeBuffers();
        }

        private static void DisposeBuffers()
        {
            foreach (var item in s_meshBuffers)
            {
                item.Value.DisposeAllBuffers();
            }

            s_meshBuffers.Clear();
        }

        public MeshDataBuffersReader(int maxVerticesCount, MonoBehaviour coroutineExecutor)
        {
            _maxVerticesCount = maxVerticesCount;
            _coroutineExecutor = coroutineExecutor;
        }

        public MeshBuffers GetOrCreateNewMeshBuffers()
        {
            if (s_meshBuffers.ContainsKey(_maxVerticesCount))
            {
                return s_meshBuffers[_maxVerticesCount];
            }
            else
            {
                var newMeshBuffers = new MeshBuffers(_maxVerticesCount);
                s_meshBuffers.Add(_maxVerticesCount, newMeshBuffers);
                return newMeshBuffers;
            }
        }

        public void UpdatePolygonsCount()
        {
            var meshBuffers = GetOrCreateNewMeshBuffers();

            var argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(meshBuffers.PolygonsCounter, argBuffer, 0);
            argBuffer.GetData(_polygonsCount);
            VerticesCount = PolygonsCount * 3;
        }

        public async System.Threading.Tasks.Task<DisposableMeshData> GetAllDataFromBuffers()
        {
            var meshBuffers = GetOrCreateNewMeshBuffers();
            DisposableMeshData meshDataHandler = new DisposableMeshData();

            if (PolygonsCount != 0)
            {
                _coroutinesCount = 3;

                AsyncGPUReadbackRequest verticesRequest = AsyncGPUReadback.Request(
                    meshBuffers.VerticesBuffer, sizeof(float) * 3 * VerticesCount, 0, 
                    x => meshDataHandler.UpdateVertices(x.GetData<Vector3>()));
 
                AsyncGPUReadbackRequest trianglesRequest = AsyncGPUReadback.Request(
                    meshBuffers.TrianglesBuffer, sizeof(int) * 2 * VerticesCount, 0,
                    x => meshDataHandler.UpdateTriangles(x.GetData<TriangleAndMaterialHash>()));

                AsyncGPUReadbackRequest uvsRequest = AsyncGPUReadback.Request(
                    meshBuffers.UVBuffer, sizeof(float) * 2 * VerticesCount, 0,
                    x => meshDataHandler.UpdateUVs(x.GetData<Vector2>()));

                await UniTask.WaitWhile(() => !meshDataHandler.IsAllArraysUpdated(), PlayerLoopTiming.PreUpdate);
            }

            return meshDataHandler;
        }

        private IEnumerator ReceivingDataProcess<T>(
            AsyncGPUReadbackRequest request, Action<NativeArray<T>> dataReceivedCallback)
            where T : struct
        {
            _coroutinesCount++;
            while (!request.done) 
            {
                yield return null;
            }

            dataReceivedCallback(request.GetData<T>());
            _coroutinesCount--;
        }
    }
}
