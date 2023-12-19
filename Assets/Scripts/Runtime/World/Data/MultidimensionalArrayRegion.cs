using System.Runtime.CompilerServices;
using UnityEngine;

namespace World.Data
{
    public class MultidimensionalArrayRegion<T>
    {
        private readonly MultidimensionalArray<T> _array;
        private readonly Vector3Int _regionSize;
        private readonly Vector3Int _regionOffset;

        public Vector3Int RegionSize => _regionSize;

        public MultidimensionalArrayRegion(MultidimensionalArray<T> array, Vector3Int regionSize, Vector3Int regionOffset)
        {
            _array = array;
            _regionSize = regionSize;
            _regionOffset = regionOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int x, int y, int z)
        {
            return _array.GetValue(
                _regionOffset.x + x, _regionOffset.y + y, _regionOffset.z + z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int y, int z, T value)
        {
            _array.SetValue(
                _regionOffset.x + x, _regionOffset.y + y, _regionOffset.z + z, value);
        }
    }
}
