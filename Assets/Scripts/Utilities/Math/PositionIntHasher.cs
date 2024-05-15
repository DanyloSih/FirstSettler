using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public static class PositionIntHasher
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
        /// <summary>
        /// <inheritdoc cref="X_UNSIGNED_LIMIT"/>
        /// </summary>
        public const int X_MAX =  (X_UNSIGNED_LIMIT / 2);
        /// <summary>
        /// <inheritdoc cref="Y_UNSIGNED_LIMIT"/>
        /// </summary>
        public const int Y_MAX =  (Y_UNSIGNED_LIMIT / 2);
        /// <summary>
        /// <inheritdoc cref="Z_UNSIGNED_LIMIT"/>
        /// </summary>
        public const int Z_MAX =  (Z_UNSIGNED_LIMIT / 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetPositionFromHash(int hash)
        {
            int z = (hash & Z_UNSIGNED_LIMIT) - Z_MAX;
            int y = ((hash >> 11) & Y_UNSIGNED_LIMIT) - Y_MAX;
            int x = ((hash >> 21) & X_UNSIGNED_LIMIT) - X_MAX;

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// This hash function guarantees unique values if the axis 
        /// arguments are within the "collision limits": <br/>
        /// <inheritdoc cref="X_UNSIGNED_LIMIT"/> <br/>
        /// <inheritdoc cref="Y_UNSIGNED_LIMIT"/> <br/>
        /// <inheritdoc cref="Z_UNSIGNED_LIMIT"/> <br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashFromPosition(int x, int y, int z)
        {
            int xFormated = (((x + X_MAX) % X_UNSIGNED_LIMIT) & X_UNSIGNED_LIMIT) << 21;
            int yFormated = (((y + Y_MAX) % Y_UNSIGNED_LIMIT) & Y_UNSIGNED_LIMIT) << 11;
            int zFormated = ((z + Z_MAX) % Z_UNSIGNED_LIMIT) & Z_UNSIGNED_LIMIT;
            return xFormated | yFormated | zFormated;
        }

        /// <summary>
        /// <inheritdoc cref="GetHashFromPosition(int, int, int)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashFromPosition(Vector3Int position)
        {
            return GetHashFromPosition(position.x, position.y, position.z);
        }
    }
}