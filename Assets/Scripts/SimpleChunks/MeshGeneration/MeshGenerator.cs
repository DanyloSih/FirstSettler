using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Utilities.Math;
using Utilities.Threading.Extensions;

namespace SimpleChunks.MeshGeneration
{
    public abstract class MeshGenerator : MonoBehaviour
    {
        private static Thread s_mainThread = Thread.CurrentThread;

        private Queue<MeshGenerationArgs> _generationTasks = new();
        private Task<MeshData[]> _currentTask;

        public bool IsGenerating { get => _currentTask != null && !_currentTask.IsCompleted; }

        public Task<MeshData[]> GenerateMeshDataForChunks(
            NativeArray<Vector3Int> positions,
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData,
            CancellationToken? cancellationToken = null)
        {
            TaskCompletionSource<MeshData[]> tcs = new();
            _generationTasks.Enqueue(new MeshGenerationArgs(
                positions, chunksData, cancellationToken, tcs));

            InvokeNextTask();

            return tcs.Task;
        }

        protected bool CheckIsMainThread()
        {
            bool isMainThread = s_mainThread == Thread.CurrentThread;
            Debug.Log($"Is main thread: {isMainThread}");
            return isMainThread;
        }

        protected abstract Task<MeshData[]> OnGenerateMeshDataForChunks(
            NativeArray<Vector3Int> positions,
            NativeParallelHashMap<int, UnsafeNativeArray<VoxelData>>.ReadOnly chunksData,
            CancellationToken? cancellationToken = null);

        protected void CheckIsGenerating()
        {
            if (IsGenerating)
            {
                throw new InvalidOperationException($"It is impossible to start a new generation process " +
                    $"until the previous one is completed!");
            }
        }

        private async void InvokeNextTask()
        {
            if (_generationTasks.Count <= 0 || IsGenerating)
            {
                return;
            }

            MeshGenerationArgs nextTaskArgs = _generationTasks.Dequeue();
            if (nextTaskArgs.CancellationToken.IsCanceled())
            {
                InvokeNextTask();
                return;
            }
            _currentTask = OnGenerateMeshDataForChunks(nextTaskArgs.Positions, nextTaskArgs.ChunksData, nextTaskArgs.CancellationToken);

            Task task = _currentTask.ContinueWith((result) => {
                if (result.IsCanceled)
                {
                    nextTaskArgs.TaskCompletionSource.SetCanceled();
                }
            });

            try
            {
                var result = await _currentTask;
                nextTaskArgs.TaskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                nextTaskArgs.TaskCompletionSource.SetException(ex);
                throw ex;
            }
            finally
            {
                InvokeNextTask();
            }
        }
    }
}
