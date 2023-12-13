using System.Runtime.CompilerServices;

namespace MarchingCubesProject
{
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    public class ChunkData
    {
        private readonly float[] _voxels;
        private readonly int[] _materials;
        private bool _flipNormals;

        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        private readonly int _strideY;
        private readonly int _strideZ;

        public ChunkData(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;

            _strideY = _width;
            _strideZ = _width * _height;

            _voxels = new float[_width * _height * _depth];
            _materials = new int[_width * _height * _depth];
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

#region Volume
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetVolume(int x, int y, int z)
        {
            return _voxels[XYZToIndex(x, y, z)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetVolume(int index)
        {
            return _voxels[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVolume(int x, int y, int z, float value)
        {
            _voxels[XYZToIndex(x, y, z)] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVolume(int index, float value)
        {
            _voxels[index] = value;
        }
#endregion

#region MaterialHash
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaterialHash(int x, int y, int z)
        {
            return _materials[XYZToIndex(x, y, z)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaterialHash(int index)
        {
            return _materials[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaterialHash(int x, int y, int z, int value)
        {
            _materials[XYZToIndex(x, y, z)] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaterialHash(int index, int value)
        {
            _materials[index] = value;
        }
#endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int XYZToIndex(int x, int y, int z)
        {
            return x + y * _strideY + z * _strideZ;
        }
    }
}
