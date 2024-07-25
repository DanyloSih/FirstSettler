using Unity.Collections;
using UnityEngine;
using Utilities.Math;

namespace FirstSettler.Tests
{
    public class UnsafeNativeArrayTest : MonoBehaviour
    {
        private UnsafeNativeArray<int> _unsafeNativeArray;

        private void Start()
        {
            var nativeArray = new NativeArray<int>(10, Allocator.Persistent);

            _unsafeNativeArray = new UnsafeNativeArray<int>(nativeArray, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            _unsafeNativeArray.Dispose();
        }
    } 
}
