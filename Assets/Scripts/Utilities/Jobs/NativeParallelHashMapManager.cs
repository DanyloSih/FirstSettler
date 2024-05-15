using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Utilities.Threading.Extensions;

namespace Utilities.Jobs
{
    public delegate TaskCompletionSource<bool> ReadParallelHashMapDelegate<TKey, TValue>(
            NativeParallelHashMap<TKey, TValue>.ReadOnly readingHashMap)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged;

    public delegate void EditParallelHashMapDelegate<TKey, TValue>(
            NativeParallelHashMap<TKey, TValue> writingHashMap)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged;

    public class NativeParallelHashMapManager<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private Queue<ReadParallelHashMapDelegate<TKey, TValue>> _readFunctionsQueue = new();
        private Queue<EditParallelHashMapDelegate<TKey, TValue>> _editFunctionsQueue = new();
        
        private NativeParallelHashMap<TKey, TValue> _parallelHashMap;
        private TaskCompletionSource<bool> _readingTask;

        public NativeParallelHashMapManager(int capacity)
        {
            _parallelHashMap = new NativeParallelHashMap<TKey, TValue>(capacity, Allocator.Persistent);
        }

        public void GetReadOnly(ReadParallelHashMapDelegate<TKey, TValue> readFunction)
        {
            _readFunctionsQueue.Enqueue(readFunction);

            if (_readingTask.IsCompleted())
            {
                StartReading();
            }
        }

        public void Add(TKey key, TValue value)
        {
            _editFunctionsQueue.Enqueue((hashMap) => hashMap.Add(key, value));

            if (_readingTask.IsCompleted())
            {
                ApplyEditFunctions();
            }
        }

        public void Remove(TKey key)
        {
            _editFunctionsQueue.Enqueue((hashMap) => hashMap.Remove(key));

            if (_readingTask.IsCompleted())
            {
                ApplyEditFunctions();
            }
        }

        public void Dispose()
        {
            _parallelHashMap.Dispose();
        }

        private async void StartReading()
        {
            ApplyEditFunctions();

            if (_readFunctionsQueue.Count == 0)
            {
                return;
            }

            ReadParallelHashMapDelegate<TKey, TValue> readFunction = _readFunctionsQueue.Dequeue();
            _readingTask = readFunction.Invoke(_parallelHashMap.AsReadOnly());
            try
            {
                await _readingTask.Task;
            }
            catch (TaskCanceledException taskCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                StartReading();
            }
        }

        private void ApplyEditFunctions()
        {
            for (int i = 0; i < _editFunctionsQueue.Count; i++)
            {
                EditParallelHashMapDelegate<TKey, TValue> editFunction = _editFunctionsQueue.Dequeue();
                editFunction.Invoke(_parallelHashMap);
            }
        }
    }
}