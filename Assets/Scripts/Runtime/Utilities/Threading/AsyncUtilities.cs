using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Threading
{
    public static class AsyncUtilities
    {
        /// <summary>
        /// Freezes the flow until the passed condition equals true.
        /// </summary>
        /// <param name="condition">Will be awaiting while this condition is true.</param>
        /// <param name="waitDelayInMilliseconds">Check condition iteration delay in milliseconds</param>
        /// <param name="cancellationToken">Stops waiting if a cancellation request has been received.</param>
        /// <returns></returns>
        public static async Task WaitWhile(
            Func<bool> condition, 
            int waitDelayInMilliseconds = 10, 
            CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                while (condition())
                {
                    await Task.Delay(waitDelayInMilliseconds);
                }
            }
            else
            {
                CancellationToken token = cancellationToken.Value;
                while (condition() && !token.IsCancellationRequested)
                {
                    await Task.Delay(waitDelayInMilliseconds);
                }
            }
        }
    }
}