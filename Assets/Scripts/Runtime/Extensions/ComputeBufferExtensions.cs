using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FirstSettler.Extensions
{
    public static class ComputeBufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComputeBuffer Create(int bufferLength, Type dataType)
        {
            if (dataType.IsPrimitive)
            {
                return new ComputeBuffer(bufferLength, Marshal.SizeOf(dataType));
            }
            else
            {
                int dataSize = 0;
                foreach (var field in dataType.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    dataSize += Marshal.SizeOf(field.DeclaringType);
                }
                return new ComputeBuffer(bufferLength, dataSize);
            }
        }
    }
}