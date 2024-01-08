using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public static class PositionHasher
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetPositionHash(int x, int y, int z)
        {
            unchecked
            {
                long hash = 54;
                hash = hash * 228 + x.GetHashCode();
                hash = hash * 228 + y.GetHashCode();
                hash = hash * 228 + z.GetHashCode();
                return hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetPositionHash(Vector3Int position)
        {
            return GetPositionHash(position.x, position.y, position.z);
        }
    }
}