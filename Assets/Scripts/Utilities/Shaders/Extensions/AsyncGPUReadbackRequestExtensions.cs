using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Utilities.Threading;

namespace Utilities.Shaders.Extensions
{
    public static class AsyncGPUReadbackRequestExtensions
    {
        public static bool HasError(
            this AsyncGPUReadbackRequest[] requests)
        {
            for (int i = 0; i < requests.Length; i++)
            {
                if (requests[i].hasError)
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task WaitUntilDone(
           this AsyncGPUReadbackRequest request,
           int delayInMilliseconds = 1,
           CancellationToken? cancellationToken = null)
        {
            await AsyncUtilities.WaitWhile(() => !request.done, delayInMilliseconds, cancellationToken);
        }

        public static async Task WaitUntilDone(
            this AsyncGPUReadbackRequest[] requests, 
            int delayInMilliseconds = 1,
            CancellationToken? cancellationToken = null)
        {
            await AsyncUtilities.WaitWhile(() => !IsRequestsDone(requests), delayInMilliseconds, cancellationToken);
        }

        public static async Task WaitUntilDoneWithTimeout(
            this AsyncGPUReadbackRequest request,
            int delayInMilliseconds = 1,
            int timeoutInMilliseconds = 20,
            CancellationToken? cancellationToken = null)
        {
            await AsyncUtilities.WaitWhileWithTimeout(
                () => !request.done, delayInMilliseconds, timeoutInMilliseconds, cancellationToken);
        }

        public static async Task WaitUntilDoneWithTimeout(
            this AsyncGPUReadbackRequest[] requests,
            int delayInMilliseconds = 1,
            int timeoutInMilliseconds = 20,
            CancellationToken? cancellationToken = null)
        {
            await AsyncUtilities.WaitWhileWithTimeout(
                () => !IsRequestsDone(requests), delayInMilliseconds, timeoutInMilliseconds, cancellationToken);
        }

        private static bool IsRequestsDone(AsyncGPUReadbackRequest[] requests)
        {
            for (int i = 0; i < requests.Length; i++)
            {
                if (!requests[i].done)
                {
                    return false;
                }
            }

            return true;
        }
    }

}