using System;
using System.Threading.Tasks;

namespace Utilities.Threading.Extensions
{
    public static class TaskExetensions
    {
        public static Task OnException(this Task task, Action<Exception> onExceptionCallback)
        {
            task.ContinueWith((result) => { 
                if (result.Exception != null)
                {
                    onExceptionCallback(result.Exception); 
                }
            });

            return task;
        }

        public static Task<T> OnException<T>(this Task<T> task, Action<Exception> onExceptionCallback)
        {
            task.ContinueWith((result) => {
                if (result.Exception != null)
                {
                    onExceptionCallback(result.Exception);
                }
            });

            return task;
        }
    }
}
