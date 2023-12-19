using System.Runtime.CompilerServices;

namespace World.Data
{
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    public class ChunkData
    {
        private readonly MultidimensionalArrayRegion<VoxelData> _voxelsData;
        private bool _flipNormals;

        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        public ChunkData(
            MultidimensionalArrayRegion<VoxelData> voxelsData)
        {
            _voxelsData = voxelsData;
            _width = _voxelsData.RegionSize.x;
            _height = _voxelsData.RegionSize.y;
            _depth = _voxelsData.RegionSize.z;
            _flipNormals = true;
        }

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
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
