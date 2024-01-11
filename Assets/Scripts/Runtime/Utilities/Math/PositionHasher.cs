using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public static class PositionHasher
    {
        /// <summary>
        /// X axis limits (-1023, 1023)
        /// </summary>
        public const int X_UNSIGNED_LIMIT = 2047;
        /// <summary>
        /// Y axis limit (-511, 511)
        /// </summary>
        public const int Y_UNSIGNED_LIMIT = 1023;
        /// <summary>
        /// Z axis limit (-1023, 1023)
        /// </summary>
        public const int Z_UNSIGNED_LIMIT = 2047;

        private const int _X_MAX_LIMIT =  (X_UNSIGNED_LIMIT / 2);
        private const int _X_MIN_LIMIT = -(X_UNSIGNED_LIMIT / 2);

        private const int _Y_MAX_LIMIT =  (Y_UNSIGNED_LIMIT / 2);
        private const int _Y_MIN_LIMIT = -(Y_UNSIGNED_LIMIT / 2);

        private const int _Z_MAX_LIMIT =  (Z_UNSIGNED_LIMIT / 2);
        private const int _Z_MIN_LIMIT = -(Z_UNSIGNED_LIMIT / 2);

        /// <summary>
        /// This hash function guarantees unique values if the axis 
        /// arguments are within the "collision limits": <br/>
        /// <inheritdoc cref="X_UNSIGNED_LIMIT"/> <br/>
        /// <inheritdoc cref="Y_UNSIGNED_LIMIT"/> <br/>
        /// <inheritdoc cref="Z_UNSIGNED_LIMIT"/> <br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPositionHash(int x, int y, int z)
        {
            int xFormated = (((x + _X_MIN_LIMIT) % _X_MAX_LIMIT) & X_UNSIGNED_LIMIT) << 21;
            int yFormated = (((y + _Y_MIN_LIMIT) % _Y_MAX_LIMIT) & Y_UNSIGNED_LIMIT) << 11;
            int zFormated = ((z + _Z_MIN_LIMIT) % _Z_MAX_LIMIT) & Z_UNSIGNED_LIMIT;
            return xFormated | yFormated | zFormated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPositionHash(Vector3Int position)
        {
            return GetPositionHash(position.x, position.y, position.z);
        }
    }
}