using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FirstSettler.Extensions;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities.Math;
using Utilities.Threading;

namespace World.Data
{
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    public class ChunkData : IDisposable
    {
        private ThreedimensionalNativeArray<VoxelData> _voxelsData;
        private bool _flipNormals;
        private ComputeBuffer _voxelsBuffer;

        private readonly Vector3Int _size;
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        public ChunkData(Vector3Int chunkSize)
        {
            _voxelsData = new ThreedimensionalNativeArray<VoxelData>(chunkSize + new Vector3Int(1, 1, 1));
            _size = _voxelsData.Size;
            _width = _voxelsData.Size.x;
            _height = _voxelsData.Size.y;
            _depth = _voxelsData.Size.z;
            _flipNormals = true;
        }

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public Vector3Int Size => _size;
        public ThreedimensionalNativeArray<VoxelData> VoxelsData => _voxelsData;
        public bool FlipNormals
        {
            get => _flipNormals;
            set => _flipNormals = value;
        }

        /// <param name="startPosition">Start position of data copying.</param>
        /// <param name="size">Size of the copied area.</param>
        public ChunkData CopyPart(
            Vector3Int startPosition,
            Vector3Int size,
            int innerLoopBatchCount = 1)
        {
            var result = new ChunkData(size);
            var jobHandle = VoxelsData.CreateCopyPartJob(startPosition, result.VoxelsData, innerLoopBatchCount);
            jobHandle.Complete();
            return result;
        }

        /// <param name="startPosition">Start position of data copying.</param>
        /// <param name="size">Size of the copied area.</param>
        public async Task<ChunkData> CopyPartAsync(
            Vector3Int startPosition,
            Vector3Int size,
            int innerLoopBatchCount = 1,
            int waitDelayInMilliseconds = 1)
        {
            var result = new ChunkData(size);
            JobHandle jobHandle = VoxelsData.CreateCopyPartJob(startPosition, result.VoxelsData, innerLoopBatchCount);
            await AsyncUtilities.WaitWhile(() => !jobHandle.IsCompleted, waitDelayInMilliseconds);
            jobHandle.Complete();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComputeBuffer GetOrCreateVoxelsDataBuffer()
        {
            _voxelsBuffer = _voxelsBuffer ?? ComputeBufferExtensions.Create(VoxelsData.FullLength, typeof(VoxelData));
            _voxelsBuffer.SetData(VoxelsData.RawData);
            return _voxelsBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task GetDataFromVoxelsBuffer(ComputeBuffer dataComputeBuffer)
        {
            AsyncGPUReadbackRequest request = AsyncGPUReadback.RequestIntoNativeArray(
                ref _voxelsData.RawData, dataComputeBuffer);

            await AsyncUtilities.WaitWhile(() => !request.done, 5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VoxelData GetVoxelData(int x, int y, int z)
        {
            return _voxelsData.GetValue(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VoxelData GetVoxelData(Vector3Int voxelLocalPosition)
        {
            return _voxelsData.GetValue(
                voxelLocalPosition.x, voxelLocalPosition.y, voxelLocalPosition.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVoxelData(int x, int y, int z, VoxelData value)
        {
            _voxelsData.SetValue(x, y, z, value);
        }

        public void Dispose()
        {
            if(_voxelsBuffer != null )
            {
                _voxelsBuffer.Dispose();
            }

            _voxelsData.RawData.Dispose();
        }
    }
}
