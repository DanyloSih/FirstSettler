using System.Runtime.CompilerServices;
using UnityEngine;

namespace World.Data
{
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    public class ChunkData
    {
        private readonly MultidimensionalArray<VoxelData> _voxelsData;
        private readonly Vector3Int _size;
        private bool _flipNormals;

        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        public ChunkData(
            MultidimensionalArray<VoxelData> voxelsData)
        {
            _voxelsData = voxelsData;
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
        public MultidimensionalArray<VoxelData> VoxelsData => _voxelsData;
        public bool FlipNormals
        {
            get => _flipNormals;
            set => _flipNormals = value;
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
    }
}
