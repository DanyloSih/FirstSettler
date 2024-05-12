using System;

namespace Utilities.Threading
{
    public struct WaitResult
    {
        public bool IsCanceled;
        public bool IsTimeOut;
        public Exception Exception;

        public WaitResult(bool isCanceled, bool isTimeOut, Exception exception)
        {
            IsCanceled = isCanceled;
            IsTimeOut = isTimeOut;
            Exception = exception;
        }

        public bool IsWaitedSuccessfully => Exception == null && !IsCanceled && !IsTimeOut;
    }
}