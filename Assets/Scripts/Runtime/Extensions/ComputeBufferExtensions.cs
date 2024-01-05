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
        public static ComputeBuffer Create(
            int bufferLength,
            Type dataType,
            ComputeBufferType type = ComputeBufferType.Default,
            ComputeBufferMode usage = ComputeBufferMode.Immutable)
        {
            return new ComputeBuffer(bufferLength, Marshal.SizeOf(dataType), type, usage);
        }
    }
}