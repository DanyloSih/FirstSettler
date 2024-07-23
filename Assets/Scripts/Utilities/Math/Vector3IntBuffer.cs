using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utilities.Math
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector3IntBuffer
    {
        [FieldOffset(0)]
        private int _vectorAxes;
        [FieldOffset(sizeof(int) * 3)]
        private byte _voxelNeighborsEnd;

        public int this[int axisId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    fixed (int* ptr = &_vectorAxes)
                    {
                        return *(ptr + axisId);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                unsafe
                {
                    fixed (int* ptr = &_vectorAxes)
                    {
                        *(ptr + axisId) = value;
                    }
                }
            }
        }

        public Vector3IntBuffer(Vector3Int vector3Int) : this()
        {
            Set(vector3Int);
        }

        public Vector3IntBuffer(Vector3IntBuffer vector3IntBuffer) : this()
        {
            Set(vector3IntBuffer);
        }

        public Vector3IntBuffer(int x, int y, int z) : this()
        {
            Set(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, int z)
        {
            this[0] = x;
            this[1] = y;
            this[2] = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Vector3Int vector3Int)
        {
            this[0] = vector3Int.x;
            this[1] = vector3Int.y;
            this[2] = vector3Int.z;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Vector3IntBuffer vector3IntBuffer)
        {
            this[0] = vector3IntBuffer[0];
            this[1] = vector3IntBuffer[1];
            this[2] = vector3IntBuffer[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(this[0], this[1], this[2]);
        }
    }

}