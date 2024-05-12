using System;
using UnityEngine;
using Utilities.Common;

namespace Utilities.Shaders
{
    public class ComputeBufferManager : DisposableCollectionManager<ComputeBuffer>
    {
        public ComputeBufferManager(Func<int, ComputeBuffer> createBufferFunction) 
            : base(createBufferFunction)
        {
        }
    }
}