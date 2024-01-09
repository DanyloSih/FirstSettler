using System;
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
        /// <returns></returns>
        public static async Task WaitWhile(Func<bool> condition, int waitDelayInMilliseconds = 10)
        {
            while (condition())
            {
                await Task.Delay(waitDelayInMilliseconds);
            }
        }
    }
}