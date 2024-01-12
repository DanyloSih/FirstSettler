using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Utilities.Math
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SubnodesMapping
    {
        public const int CAPACITY = 8;

        [FieldOffset(0)]
        private int _length;
        [FieldOffset(4)]
        private int _mappingBufferStart;
        [FieldOffset(4 + CAPACITY * sizeof(int))]
        private byte _mappingBufferEnd;

        public int this[int id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe 
                {
                    fixed (int* ptr = &_mappingBufferStart)
                    {
                        return *(ptr + id);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set
            {
                unsafe
                {
                    fixed (int* ptr = &_mappingBufferStart)
                    {
                        *(ptr + id) = value;
                    }
                }
            }
        }

        public int Length 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length; 
        }

        /// <summary>
        /// WARNING!!! <br/> 
        /// This method don't check is array full, <br/>
        /// which can lead to changes in data outside the scope of this structure.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeAddValue(int hash)
        {
            this[_length] = hash;
            _length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsContais(int hash)
        {
            for (int i = 0; i < _length; i++)
            {
                if (this[i] == hash)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveByValue(int hash)
        {
            for (int i = 0; i < _length; i++)
            {
                if (this[i] == hash)
                {
                    UnsafeRemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// WARNING!!! <br/> 
        /// This method does not check whether the specified index is out of bounds of the array, <br/>
        /// which can lead to changes in data outside the scope of this structure.
        /// </summary>
        public void UnsafeRemoveAt(int index)
        {
            for (int i = index + 1; i < CAPACITY; i++)
            {
                this[i - 1] = this[i];
            }

            _length--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _length = 0;
        }
    }
}