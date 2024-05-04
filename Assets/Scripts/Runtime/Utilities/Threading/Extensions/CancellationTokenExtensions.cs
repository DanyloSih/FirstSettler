using System.Threading;

namespace Utilities.Threading.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static bool IsCanceled(this CancellationToken? cancellationToken)
        {
            return cancellationToken != null && cancellationToken.Value.IsCancellationRequested;
        }
    }
}
