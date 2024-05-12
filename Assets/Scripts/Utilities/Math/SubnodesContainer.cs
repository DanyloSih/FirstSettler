using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;

namespace Utilities.Math
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SubnodesContainer
    {
        public const int CAPACITY = 8;

        [FieldOffset(0)]
        private int _length;
        [FieldOffset(sizeof(int))]
        public int ArrayStart;
        [FieldOffset(sizeof(int) + CAPACITY * sizeof(int))]
        private byte _arrayEnd;

        private int this[int id]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe 
                {
                    fixed (int* ptr = &ArrayStart)
                    {
                        return *(ptr + id);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                unsafe
                {
                    fixed (int* ptr = &ArrayStart)
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
        /// This method DON'T CHECK is specified index is out of bounds of the array. <br/>
        /// If the index is out of bounds, the result may be unexpected.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UnsafeGetValueAt(int index)
        {
            return this[index];
        }

        /// <summary>
        /// WARNING!!! <br/> 
        /// This method DON'T CHECK is array full, <br/>
        /// which can lead to changes in data outside the scope of this structure.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeAddValue(int hash)
        {
            this[_length] = hash;
            _length++;
        }

        /// <summary>
        /// WARNING!!! <br/> 
        /// This method DON'T CHECK is specified index is out of bounds of the array, <br/>
        /// which can lead to changes in data outside the scope of this structure.
        /// </summary>
        public void UnsafeRemoveAt(int index)
        {
            unsafe
            {
                fixed (int* ptr = &ArrayStart)
                {
                    int indexValue = *(ptr + index);
                    for (int i = index + 1; i < _length; i++)
                    {                    
                         *(ptr + i - 1) = *(ptr + i);
                    }
                    *(ptr + _length - 1) = indexValue;
                }
            }    
            _length--;
        }

        /// <summary>
        /// This method CHECK is array full before adding value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue(int hash)
        {
            if (_length >= CAPACITY)
            {
                throw new IndexOutOfRangeException();
            }

            UnsafeAddValue(hash);
        }

        /// <summary>
        /// WARNING!!! <br/> 
        /// This method CHECK is specified index is out of bounds of the array, <br/>
        /// before changing data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _length)
            {
                throw new IndexOutOfRangeException();
            }

            UnsafeRemoveAt(index);  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllValuesEqual()
        {
            if (_length < 1)
            {
                return true;
            }
            else
            {
                if (_length != CAPACITY)
                {
                    return false;
                }
                else
                {
                    unsafe
                    {
                        fixed (int* ptr = &ArrayStart)
                        {
                            int firstHash = *(ptr);
                            long hashSum 
                                = firstHash
                                + *(ptr + 1)
                                + *(ptr + 2)
                                + *(ptr + 3)
                                + *(ptr + 4)
                                + *(ptr + 5)
                                + *(ptr + 6)
                                + *(ptr + 7);

                            return (hashSum / firstHash - 8) + (hashSum % firstHash) == 0;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsContains(int hash)
        {
            unsafe
            {
                fixed (int* ptr = &ArrayStart)               
                {
                    for (int i = 0; i < _length; i++)
                    {
                        if (*(ptr + i) == hash)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveByValue(int hash)
        {
            unsafe
            {
                fixed (int* ptr = &ArrayStart)
                {
                    for (int i = 0; i < _length; i++)
                    {
                        if (*(ptr + i) == hash)
                        {
                            UnsafeRemoveAt(i);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _length = 0;
        }
    }
}