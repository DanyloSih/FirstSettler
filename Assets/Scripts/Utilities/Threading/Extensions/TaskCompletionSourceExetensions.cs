using System.Threading.Tasks;

namespace Utilities.Threading.Extensions
{
    public static class TaskCompletionSourceExetensions
    {
        public static bool IsCompleted<T>(this TaskCompletionSource<T> completionSource)
        {
            return completionSource == null || completionSource.Task.IsCompleted();
        }
    }
}
