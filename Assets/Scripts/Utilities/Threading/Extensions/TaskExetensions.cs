using System;
using System.Threading.Tasks;

namespace Utilities.Threading.Extensions
{

    public static class TaskExetensions
    {
        public static bool IsCompleted(this Task task)
        {
            return task == null || task.IsCompleted;
        }

        public static bool IsCompleted<T>(this Task<T> task)
        {
            return task == null || task.IsCompleted;
        }

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
