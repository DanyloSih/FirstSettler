using System;
using Unity.Collections;
using Utilities.Common;

namespace Utilities.Jobs
{
    public class NativeArrayManager<T> : DisposableCollectionManager<NativeArray<T>>
        where T : unmanaged
    {
        public NativeArrayManager(Func<int, NativeArray<T>> createBufferFunction) 
            : base(createBufferFunction)
        {
        }

        protected override bool DisposeCondition(NativeArray<T> disposableObject)
        {
            return disposableObject.IsCreated;
        }
    }

}