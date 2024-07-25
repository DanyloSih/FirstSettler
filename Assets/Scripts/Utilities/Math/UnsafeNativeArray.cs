using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utilities.Math
{
    /// <summary>
    /// An unsafe interface for using NativeArray as a nesting within other native structures.
    /// </summary>
    public unsafe struct UnsafeNativeArray<T> : IDisposable
        where T : unmanaged
    {
        public bool IsCreated { get; private set; } 
        public readonly T* Pointer;
        public readonly int Length;
        private readonly Allocator Allocator;

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

        public UnsafeNativeArray(NativeArray<T> target, Allocator allocator)
        {
            Pointer = (T*)target.GetUnsafePtr();
            Length = target.Length;
            Allocator = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(target);
#endif

            IsCreated = true;
        }

        public NativeArray<T> RestoreAsArray()
        {
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(Pointer, Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, _safetyHandle);
#endif
            return nativeArray;
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            UnsafeUtility.FreeTracked(Pointer, Allocator);
            AtomicSafetyHandle.Release(_safetyHandle);
#else
            UnsafeUtility.Free(Pointer, Allocator);
#endif
            IsCreated = false;
        }
    }

}