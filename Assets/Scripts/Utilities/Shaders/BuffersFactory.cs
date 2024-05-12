using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utilities.Shaders
{
    public static class BuffersFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComputeBuffer CreateCompute(
            int bufferLength,
            Type dataType,
            ComputeBufferType type = ComputeBufferType.Default,
            ComputeBufferMode usage = ComputeBufferMode.Immutable)
        {
            return new ComputeBuffer(bufferLength, Marshal.SizeOf(dataType), type, usage);
        }
    }

}