using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Threading.Extensions;

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
        public static async Task<WaitResult> WaitWhile(
            Func<bool> condition, 
            int waitDelayInMilliseconds = 10, 
            CancellationToken? cancellationToken = null)
        {
            try
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
            catch (Exception ex)
            {
                return new WaitResult(cancellationToken.IsCanceled(), false, ex);
            }

            return new WaitResult(cancellationToken.IsCanceled(), false, null);
        }

        public static async Task<WaitResult> WaitWhileWithTimeout(
            Func<bool> condition,
            int waitDelayInMilliseconds = 10,
            int timeoutInMilliseconds = 100,
            CancellationToken? cancellationToken = null)
        {
            Task<WaitResult>[] tasks = new Task<WaitResult>[] {
                WaitWhile(condition, waitDelayInMilliseconds, cancellationToken),
                WaitTime(timeoutInMilliseconds)
            };

            await Task.WhenAny(tasks);

            return tasks[0].IsCompleted ? tasks[0].Result : tasks[1].Result;
        }

        private static async Task<WaitResult> WaitTime(int waitTime)
        {
            await Task.Delay(waitTime);
            return new WaitResult(false, true, null);
        }
    }
}