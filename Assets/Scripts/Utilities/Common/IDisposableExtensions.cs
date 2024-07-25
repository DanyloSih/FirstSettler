using System;
using System.Threading.Tasks;

namespace Utilities.Common
{
    public static class IDisposableExtensions
    {
        /// <summary>
        /// Allows to wait until the object is available for disposing and then dispose it.
        /// </summary>
        public async static Task WaitForDisposing(this IDisposable disposableObject, int checkDelay = 20, int timeout = 2000)
        {
            int timer = 0;
            while (timer < timeout)
            {
                try
                {
                    disposableObject.Dispose();
                    return;
                }
                catch
                {
                    timer += checkDelay;
                    await Task.Delay(checkDelay);
                }
            }

            throw new InvalidOperationException(
                $"Unable to free an array during {timeout} seconds.");
        }
    }

}