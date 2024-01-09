using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FirstSettler.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
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
