using System.Runtime.CompilerServices;

namespace Utilities.Math
{
    public static class PositionLongHasher
    {
        public const long MAX_21_BIT_VALUE = 1048575;
        public const long MIN_21_BIT_VALUE = -1048576;
        private const int _TYPE_SIZE_MINUS_ONE = sizeof(long) * 8 - 1;
        private const int _USED_BITS_COUNT = 20;
        private const int _X_SHIFT = 42;
        private const int _Y_SHIFT = 21;
        private const int _UNUSED_BITS_COUNT = _TYPE_SIZE_MINUS_ONE - _USED_BITS_COUNT;
        private const long _SIGN_MASK = 1 << _TYPE_SIZE_MINUS_ONE;
        private const long _PACKED_SIGN_MASK = 1 << _USED_BITS_COUNT;
        private const long _USED_BITS_MASK = 1048575;
        private const long _USED_BITS_WITH_SIGN_MASK = 2097151;
        private const long _X_CLEAN_MASK = -9223367638808264705;
        private const long _Y_CLEAN_MASK = -4398044413953;
        private const long _Z_CLEAN_MASK = -2097152;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetHashFromPosition(long x, long y, long z)
        {
            long result = 0;
            result |= PackTo21Bits(x) << _X_SHIFT;
            result |= PackTo21Bits(y) << _Y_SHIFT;
            result |= PackTo21Bits(z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (long x, long y, long z) GetPositionFromHash(long hash)
        {
            (long x, long y, long z) result = new();
            result.x = UnpackFrom21Bits(hash >> _X_SHIFT);
            result.y = UnpackFrom21Bits(hash >> _Y_SHIFT);
            result.z = UnpackFrom21Bits(hash);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetXFromHash(long hash)
            => UnpackFrom21Bits(hash >> _X_SHIFT);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetXToHash(long hash, long newX)
            => (hash & _X_CLEAN_MASK) | PackTo21Bits(newX) << _X_SHIFT;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AddXToHash(long hash, long x)
            => SetXToHash(hash, GetXFromHash(hash) + x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetYFromHash(long hash)
            => UnpackFrom21Bits(hash >> _Y_SHIFT);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetYToHash(long hash, long newY)
            => (hash & _Y_CLEAN_MASK) | PackTo21Bits(newY) << _Y_SHIFT;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AddYToHash(long hash, long y)
            => SetYToHash(hash, GetYFromHash(hash) + y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetZFromHash(long hash)
            => UnpackFrom21Bits(hash);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetZToHash(long hash, long newZ)
            => (hash & _Z_CLEAN_MASK) | PackTo21Bits(newZ);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AddZToHash(long hash, long z)
            => SetZToHash(hash, GetZFromHash(hash) + z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long PackTo21Bits(long packingValue)
        {
            long sign = packingValue & _SIGN_MASK;
            long packedSign = sign >> _UNUSED_BITS_COUNT & _PACKED_SIGN_MASK;
            long usedBits = packingValue & _USED_BITS_MASK;
            long result = usedBits | packedSign;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long UnpackFrom21Bits(long unpackingValue)
        {
            long sign = unpackingValue & _PACKED_SIGN_MASK;
            long unpackedSign = sign << _UNUSED_BITS_COUNT >> _UNUSED_BITS_COUNT;
            long result = unpackingValue & _USED_BITS_WITH_SIGN_MASK | unpackedSign;
            return result;
        }
    }
}