using System;
using Unity.Collections;
using Utilities.Common;

namespace Utilities.Jobs
{
    public class NativeHashMapManager<TKey, TValue> : DisposableCollectionManager<NativeHashMap<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public NativeHashMapManager(Func<int, NativeHashMap<TKey, TValue>> createObjectFunction) : base(createObjectFunction)
        {
        }

        protected override bool DisposeCondition(NativeHashMap<TKey, TValue> disposableObject)
        {
            return disposableObject.IsCreated;
        }
    }

}