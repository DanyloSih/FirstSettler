using System;
using Unity.Collections;
using Utilities.Common;

namespace Utilities.Jobs
{
    public class NativeHashSetManager<T> : DisposableCollectionManager<NativeHashSet<T>>
        where T : unmanaged, IEquatable<T>
    {
        public NativeHashSetManager(Func<int, NativeHashSet<T>> createObjectFunction) : base(createObjectFunction)
        {
        }

        protected override bool DisposeCondition(NativeHashSet<T> disposableObject)
        {
            return disposableObject.IsCreated;
        }
    }

}