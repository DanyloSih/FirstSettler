using UnityEngine;

namespace Utilities.Shaders.Extensions
{
    public static class ComputeShaderExtensions
    {
        public static void DispatchConsideringGroupSizes(
            this ComputeShader computeShader,
            int kernelIndex,
            int threadGroupsX,
            int threadGroupsY,
            int threadGroupsZ)
        {
            uint x, y, z;

            computeShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);

            computeShader.Dispatch(
                kernelIndex, 
                Mathf.CeilToInt(threadGroupsX / (float)x),
                Mathf.CeilToInt(threadGroupsY / (float)y),
                Mathf.CeilToInt(threadGroupsZ / (float)z));
        }

        public static Vector3Int GetThreadGroupSizes(this ComputeShader computeShader, int kernelIndex)
        {
            uint x, y, z;

            computeShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);

            return new Vector3Int((int)x, (int)y, (int)z);
        }
    }

}