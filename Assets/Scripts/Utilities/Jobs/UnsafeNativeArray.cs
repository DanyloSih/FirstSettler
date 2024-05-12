using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utilities.Jobs
{
    /// <summary>
    /// An unsafe interface for using NativeArray as a nesting within other native structures.
    /// </summary>
    public unsafe struct UnsafeNativeArray<T>
        where T : unmanaged
    {
        public readonly T* Pointer;
        public readonly int Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle _safetyHandle;
#endif

        /// <summary>
        /// Iterator dont check bounds!
        /// </summary>
        public T this[int index]
        {
            get
            {
                return Pointer[index];
            }
            set
            {
                Pointer[index] = value;
            }
        }

        public UnsafeNativeArray(NativeArray<T> target)
        {
            Pointer = (T*)target.GetUnsafePtr();
            Length = target.Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(target);
#endif
        }

        public NativeArray<T> RestoreAsArray()
        {
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(Pointer, Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, _safetyHandle);
#endif
            return nativeArray;
        }
    }

}